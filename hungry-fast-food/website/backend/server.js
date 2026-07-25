// E:\hungryHub\hungry-fast-food\website\backend\server.js
// Triggering restart to load new CORS origins env


import express from 'express';
import cors from 'cors';
import dotenv from 'dotenv';
import helmet from 'helmet';
import morgan from 'morgan';
import compression from 'compression';
import { fileURLToPath } from 'url';
import { dirname, join } from 'path';
import fs from 'fs';
import { initSocketServer } from './services/socketService.js';
import { globalLimiter } from './src/middleware/rateLimiter.js';
import { xssSanitizer } from './src/middleware/xssSanitizer.js';

// Load environment variables
dotenv.config();

// Get __dirname equivalent in ES modules
const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

// Import database connection
import pool from './src/config/database.js';

// Import routes
import authRoutes from './src/routes/authRoutes.js';
import orderRoutes from './src/routes/orderRoutes.js';
import menuRoutes from './src/routes/menuRoutes.js';
import adminRoutes from './src/routes/adminRoutes.js';

// Create Express app
const app = express();
const PORT = process.env.PORT || 5000;

// ============================================
// MIDDLEWARE
// ============================================

// Security headers
app.use(helmet({
    contentSecurityPolicy: {
        directives: {
            defaultSrc: ["'self'"],
            scriptSrc: ["'self'", "'unsafe-inline'"],
            styleSrc: ["'self'", "'unsafe-inline'"],
            imgSrc: ["'self'", "data:", "https:"],
            connectSrc: ["'self'", "https:", "wss:"],
        },
    },
    crossOriginResourcePolicy: { policy: "cross-origin" }
}));

// Apply Global Rate Limiting (100 req/15 min)
app.use(globalLimiter);

// XSS Sanitization (DOMPurify on all incoming req.body/query/params)
app.use(xssSanitizer);

// CORS configuration
const allowedOrigins = process.env.ALLOWED_ORIGINS
    ? process.env.ALLOWED_ORIGINS.split(',')
    : ['http://localhost:3000', 'http://localhost:5173'];

app.use(cors({
    origin: (origin, callback) => {
        // Allow requests with no origin (like mobile apps, curl)
        if (!origin) return callback(null, true);

        if (allowedOrigins.indexOf(origin) !== -1 || process.env.NODE_ENV === 'development') {
            callback(null, true);
        } else {
            callback(new Error('Not allowed by CORS'));
        }
    },
    credentials: true,
    methods: ['GET', 'POST', 'PUT', 'DELETE', 'PATCH', 'OPTIONS'],
    allowedHeaders: ['Content-Type', 'Authorization', 'X-Requested-With']
}));

// Request logging
app.use(morgan('dev'));

// Response compression
app.use(compression());

// JSON body parser (10MB limit for images)
app.use(express.json({ limit: '10mb' }));
app.use(express.urlencoded({ extended: true, limit: '10mb' }));

// ============================================
// DATABASE CONNECTION TEST
// ============================================

async function testDatabaseConnection() {
    try {
        const client = await pool.connect();
        console.log('✅ Database connected successfully');
        client.release();
        return true;
    } catch (error) {
        console.error('❌ Database connection failed:', error.message);
        return false;
    }
}

// ============================================
// ROUTES
// ============================================

// Health check
app.get('/health', (req, res) => {
    res.status(200).json({
        status: 'ok',
        timestamp: new Date().toISOString(),
        uptime: process.uptime()
    });
});

// Temporary upload inspector
app.post('/test-upload', (req, res) => {
    let body = [];
    req.on('data', chunk => {
        body.push(chunk);
    });
    req.on('end', () => {
        const raw = Buffer.concat(body).toString('utf-8');
        const dump = `HEADERS:\n${JSON.stringify(req.headers, null, 2)}\n\nSTART SNIPPET:\n${raw.substring(0, 1000)}\n\nEND SNIPPET:\n${raw.substring(raw.length - 1000)}`;
        fs.writeFileSync('test_output.txt', dump);
        res.status(200).send('DUMPED');
    });
});

// Root endpoint
app.get('/', (req, res) => {
    res.status(200).json({
        message: '🍔 Hungry Fast Food API',
        version: '1.0.0',
        status: 'running',
        endpoints: {
            auth: '/api/auth',
            orders: '/api/orders',
            menu: '/api/menu',
            admin: '/api/admin'
        }
    });
});

// API Routes
app.use('/api/auth', authRoutes);
app.use('/api/orders', orderRoutes);
app.use('/api/menu', menuRoutes);
app.use('/api/admin', adminRoutes);

// ============================================
// ERROR HANDLING
// ============================================

// 404 - Route not found
app.use((req, res, next) => {
    res.status(404).json({
        success: false,
        message: `Route ${req.originalUrl} not found`
    });
});

// Global error handler
app.use((err, req, res, next) => {
    console.error('❌ Error:', err.stack);

    const status = err.status || 500;
    const message = err.message || 'Internal server error';

    // Send appropriate response
    res.status(status).json({
        success: false,
        message: message,
        ...(process.env.NODE_ENV === 'development' && { stack: err.stack })
    });
});

// ============================================
// START SERVER
// ============================================

async function startServer() {
    // Test database connection first
    const dbConnected = await testDatabaseConnection();

    if (!dbConnected) {
        console.error('❌ Cannot start server without database connection');
        process.exit(1);
    }

    // Start server
    app.listen(PORT, () => {
        console.log(`🍔 Server running on port ${PORT}`);
        console.log(`📍 Environment: ${process.env.NODE_ENV || 'development'}`);
        console.log(`🔗 API URL: http://localhost:${PORT}`);
        console.log(`❤️  Health check: http://localhost:${PORT}/health`);

        const socketPort = parseInt(process.env.SOCKET_SERVER_PORT || '5001', 10);
        initSocketServer(socketPort);
    });
}

// Handle uncaught exceptions
process.on('uncaughtException', (err) => {
    console.error('💥 Uncaught Exception:', err);
});

// Handle unhandled rejections
process.on('unhandledRejection', (err) => {
    console.error('💥 Unhandled Rejection:', err);
});

// Start the server
startServer();

// Graceful shutdown
process.on('SIGTERM', () => {
    console.log('📴 Received SIGTERM, shutting down gracefully...');
    process.exit(0);
});

export default app;
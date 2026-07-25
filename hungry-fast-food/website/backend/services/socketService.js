import { Server } from 'socket.io';
import http from 'http';
import dotenv from 'dotenv';

dotenv.config();

let io;

export const initSocketServer = (port) => {
  if (io) {
    console.log('🔌 Socket server already initialized');
    return io;
  }

  const httpServer = http.createServer();
  io = new Server(httpServer, {
    cors: {
      origin: process.env.ALLOWED_ORIGINS ? process.env.ALLOWED_ORIGINS.split(',') : ['http://localhost:3000', 'http://localhost:5173'],
      methods: ['GET', 'POST']
    }
  });

  io.on('connection', (socket) => {
    console.log(`🟢 Socket client connected: ${socket.id}`);

    socket.on('disconnect', () => {
      console.log(`🔴 Socket client disconnected: ${socket.id}`);
    });
  });

  httpServer.listen(port, () => {
    console.log(`📡 Socket.io server running on port ${port}`);
  });

  return io;
};

export const emitSocketEvent = (eventName, payload) => {
  if (!io) {
    console.warn('⚠️ Socket.io server is not initialized yet');
    return;
  }
  io.emit(eventName, payload);
};

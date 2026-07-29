// test-email.js
import dotenv from 'dotenv';
import nodemailer from 'nodemailer';

dotenv.config();

async function testEmail() {
    try {
        const transporter = nodemailer.createTransport({
            host: process.env.SMTP_HOST,
            port: parseInt(process.env.SMTP_PORT),
            secure: process.env.SMTP_SECURE === 'true',
            auth: {
                user: process.env.SMTP_USER,
                pass: process.env.SMTP_PASS,
            },
        });

        await transporter.verify();
        console.log('✅ Email transporter is ready');

        // Send test email
        const info = await transporter.sendMail({
            from: `"Test" <${process.env.SMTP_USER}>`,
            to: 'davidbenjamin8990@gmail.com', // Apni email daalein
            subject: 'Test Email',
            text: 'If you receive this, email is working!',
        });
 
        console.log('✅ Email sent:', info.messageId);
    } catch (error) {
        console.error('❌ Email error:', error.message);
    }
}

testEmail();
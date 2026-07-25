// E:\hungryHub\hungry-fast-food\website\backend\src\services\emailService.js

import transporter from '../config/email.js';
import dotenv from 'dotenv';

dotenv.config();

const APP_NAME = process.env.APP_NAME || 'Hungry Fast Food';
const FROM_EMAIL = process.env.SMTP_USER;

// Send OTP email
export const sendOTPEmail = async (email, otp, purpose) => {
    const html = `
    <!DOCTYPE html>
    <html>
    <head>
      <style>
        body { font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #E63946; color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }
        .content { background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }
        .otp-code { font-size: 32px; font-weight: bold; color: #E63946; text-align: center; padding: 15px; background: white; border-radius: 8px; margin: 20px 0; }
        .footer { text-align: center; color: #666; font-size: 12px; margin-top: 20px; }
      </style>
    </head>
    <body>
      <div class="header">
        <h1>🍔 ${APP_NAME}</h1>
      </div>
      <div class="content">
        <h2>OTP Verification</h2>
        <p>Hello,</p>
        <p>Please use the following OTP to ${purpose}:</p>
        <div class="otp-code">${otp}</div>
        <p>This OTP is valid for ${process.env.OTP_EXPIRY_MINUTES || 10} minutes.</p>
        <p>If you didn't request this, please ignore this email.</p>
        <p>Thanks,<br>${APP_NAME} Team</p>
      </div>
      <div class="footer">
        <p>&copy; ${new Date().getFullYear()} ${APP_NAME}. All rights reserved.</p>
      </div>
    </body>
    </html>
  `;

    return transporter.sendMail({
        from: `${APP_NAME} <${FROM_EMAIL}>`,
        to: email,
        subject: `OTP for ${purpose} - ${APP_NAME}`,
        html
    });
};

// Send Welcome email
export const sendWelcomeEmail = async (email, name) => {
    const html = `
    <!DOCTYPE html>
    <html>
    <head>
      <style>
        body { font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #E63946; color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }
        .content { background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }
        .footer { text-align: center; color: #666; font-size: 12px; margin-top: 20px; }
      </style>
    </head>
    <body>
      <div class="header">
        <h1>🍔 ${APP_NAME}</h1>
      </div>
      <div class="content">
        <h2>Welcome to ${APP_NAME}!</h2>
        <p>Dear ${name},</p>
        <p>Thank you for registering with ${APP_NAME}. We're excited to have you on board!</p>
        <p>You can now:</p>
        <ul>
          <li>Browse our delicious menu</li>
          <li>Place orders online</li>
          <li>Track your orders in real-time</li>
        </ul>
        <p>If you have any questions, feel free to contact us.</p>
        <p>Thanks,<br>${APP_NAME} Team</p>
      </div>
      <div class="footer">
        <p>&copy; ${new Date().getFullYear()} ${APP_NAME}. All rights reserved.</p>
      </div>
    </body>
    </html>
  `;

    return transporter.sendMail({
        from: `${APP_NAME} <${FROM_EMAIL}>`,
        to: email,
        subject: `Welcome to ${APP_NAME}!`,
        html
    });
};

// Send Password Reset email
export const sendPasswordResetEmail = async (email, otp) => {
    const html = `
    <!DOCTYPE html>
    <html>
    <head>
      <style>
        body { font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #E63946; color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }
        .content { background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }
        .otp-code { font-size: 32px; font-weight: bold; color: #E63946; text-align: center; padding: 15px; background: white; border-radius: 8px; margin: 20px 0; }
        .footer { text-align: center; color: #666; font-size: 12px; margin-top: 20px; }
      </style>
    </head>
    <body>
      <div class="header">
        <h1>🍔 ${APP_NAME}</h1>
      </div>
      <div class="content">
        <h2>Password Reset</h2>
        <p>Hello,</p>
        <p>You requested to reset your password. Please use the following OTP:</p>
        <div class="otp-code">${otp}</div>
        <p>This OTP is valid for ${process.env.OTP_EXPIRY_MINUTES || 10} minutes.</p>
        <p>If you didn't request this, please ignore this email or contact support.</p>
        <p>Thanks,<br>${APP_NAME} Team</p>
      </div>
      <div class="footer">
        <p>&copy; ${new Date().getFullYear()} ${APP_NAME}. All rights reserved.</p>
      </div>
    </body>
    </html>
  `;

    return transporter.sendMail({
        from: `${APP_NAME} <${FROM_EMAIL}>`,
        to: email,
        subject: `Password Reset - ${APP_NAME}`,
        html
    });
};

// Send Order Confirmation email
export const sendOrderConfirmationEmail = async (email, orderNumber, customerName, total) => {
    const html = `
    <!DOCTYPE html>
    <html>
    <head>
      <style>
        body { font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #E63946; color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }
        .content { background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }
        .order-details { background: white; padding: 15px; border-radius: 8px; margin: 15px 0; }
        .footer { text-align: center; color: #666; font-size: 12px; margin-top: 20px; }
      </style>
    </head>
    <body>
      <div class="header">
        <h1>🍔 ${APP_NAME}</h1>
      </div>
      <div class="content">
        <h2>Order Confirmation</h2>
        <p>Dear ${customerName},</p>
        <p>Thank you for your order! Your order has been placed successfully.</p>
        <div class="order-details">
          <p><strong>Order Number:</strong> ${orderNumber}</p>
          <p><strong>Total Amount:</strong> PKR ${total.toFixed(2)}</p>
          <p><strong>Status:</strong> Pending</p>
        </div>
        <p>We will notify you when your order is confirmed.</p>
        <p>Thanks,<br>${APP_NAME} Team</p>
      </div>
      <div class="footer">
        <p>&copy; ${new Date().getFullYear()} ${APP_NAME}. All rights reserved.</p>
      </div>
    </body>
    </html>
  `;

    return transporter.sendMail({
        from: `${APP_NAME} <${FROM_EMAIL}>`,
        to: email,
        subject: `Order Confirmation - ${orderNumber}`,
        html
    });
};

// Send Order Status Update email
export const sendOrderStatusEmail = async (email, orderNumber, status, customerName, reason = null) => {
    const statusMessages = {
        confirmed: 'Your order has been confirmed and is being prepared.',
        preparing: 'Your order is being prepared in the kitchen.',
        ready: 'Your order is ready and will be delivered soon.',
        completed: 'Your order has been delivered. Enjoy your meal!',
        cancelled: `Your order has been cancelled.${reason ? ` Reason: ${reason}` : ''}`
    };

    const html = `
    <!DOCTYPE html>
    <html>
    <head>
      <style>
        body { font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #E63946; color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }
        .content { background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }
        .status { font-weight: bold; color: #E63946; font-size: 18px; }
        .footer { text-align: center; color: #666; font-size: 12px; margin-top: 20px; }
      </style>
    </head>
    <body>
      <div class="header">
        <h1>🍔 ${APP_NAME}</h1>
      </div>
      <div class="content">
        <h2>Order Status Update</h2>
        <p>Dear ${customerName},</p>
        <p>Your order <strong>${orderNumber}</strong> status has been updated to:</p>
        <p class="status">${status.toUpperCase()}</p>
        <p>${statusMessages[status] || 'Your order status has been updated.'}</p>
        <p>Thanks,<br>${APP_NAME} Team</p>
      </div>
      <div class="footer">
        <p>&copy; ${new Date().getFullYear()} ${APP_NAME}. All rights reserved.</p>
      </div>
    </body>
    </html>
  `;

    return transporter.sendMail({
        from: `${APP_NAME} <${FROM_EMAIL}>`,
        to: email,
        subject: `Order ${status} - ${orderNumber}`,
        html
    });
};
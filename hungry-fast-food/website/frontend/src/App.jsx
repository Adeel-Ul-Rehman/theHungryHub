import React, { lazy, Suspense, useEffect } from 'react';
import { Routes, Route, useLocation } from 'react-router-dom';
import { FaWhatsapp } from 'react-icons/fa';
import Header from './components/common/Header';
import Footer from './components/common/Footer';
import CartFloatingButton from './components/common/CartFloatingButton';
import LoadingSpinner from './components/common/LoadingSpinner';
import ProtectedRoute from './components/common/ProtectedRoute';

// Lazy load pages for better performance
const HomePage = lazy(() => import('./components/layout/HomePage'));
const MenuPage = lazy(() => import('./components/layout/MenuPage'));
const Login = lazy(() => import('./components/auth/Login'));
const Signup = lazy(() => import('./components/auth/Signup'));
const ForgotPassword = lazy(() => import('./components/auth/ForgotPassword'));
const ResetPassword = lazy(() => import('./components/auth/ResetPassword'));
const OTPVerification = lazy(() => import('./components/auth/OTPVerification'));
const Cart = lazy(() => import('./components/cart/Cart'));
const Checkout = lazy(() => import('./components/cart/Checkout'));
const OrderTracking = lazy(() => import('./components/orders/OrderTracking'));
const OrderHistory = lazy(() => import('./components/orders/OrderHistory'));
const AboutUs = lazy(() => import('./components/pages/AboutUs'));
const ContactUs = lazy(() => import('./components/pages/ContactUs'));
const RefundPolicy = lazy(() => import('./components/pages/RefundPolicy'));
const ProductDetailPage = lazy(() => import('./components/pages/ProductDetailPage'));
const DealDetailPage = lazy(() => import('./components/pages/DealDetailPage'));

function App() {
    const { pathname } = useLocation();

    useEffect(() => {
        window.scrollTo(0, 0);
    }, [pathname]);

    return (
        <div className="min-h-screen flex flex-col bg-gray-50">
            <Header />

            <main className="flex-grow container mx-auto px-4 py-8 max-w-7xl">
                <Suspense fallback={<LoadingSpinner />}>
                    <Routes>
                        <Route path="/" element={<HomePage />} />
                        <Route path="/menu" element={<MenuPage />} />
                        <Route path="/menu/:category" element={<MenuPage />} />
                        <Route path="/about" element={<AboutUs />} />
                        <Route path="/contact" element={<ContactUs />} />
                        <Route path="/refund-policy" element={<RefundPolicy />} />
                        <Route path="/product/:id" element={<ProductDetailPage />} />
                        <Route path="/deal/:id" element={<DealDetailPage />} />

                        {/* Auth Routes */}
                        <Route path="/login" element={<Login />} />
                        <Route path="/signup" element={<Signup />} />
                        <Route path="/forgot-password" element={<ForgotPassword />} />
                        <Route path="/reset-password" element={<ResetPassword />} />
                        <Route path="/verify-otp" element={<OTPVerification />} />

                        {/* Protected Routes */}
                        <Route path="/cart" element={<Cart />} />
                        <Route path="/checkout" element={
                            <ProtectedRoute>
                                <Checkout />
                            </ProtectedRoute>
                        } />
                        <Route path="/orders" element={
                            <ProtectedRoute>
                                <OrderHistory />
                            </ProtectedRoute>
                        } />
                        <Route path="/orders/:orderNumber" element={
                            <ProtectedRoute>
                                <OrderTracking />
                            </ProtectedRoute>
                        } />
                    </Routes>
                </Suspense>
            </main>

            <CartFloatingButton />
            
            {/* WhatsApp Floating Button */}
            <a
                href="https://wa.me/923391191147"
                target="_blank"
                rel="noopener noreferrer"
                className="fixed bottom-24 right-6 md:bottom-6 md:right-6 z-40 bg-green-600 hover:bg-green-700 text-white p-3.5 sm:p-4 rounded-full shadow-2xl transition-all duration-300 hover:scale-110 active:scale-95 group flex items-center justify-center"
                aria-label="Contact us on WhatsApp"
            >
                <FaWhatsapp className="w-6 h-6 sm:w-7 sm:h-7 shrink-0" />
                {/* Visual tooltip */}
                <span className="absolute right-16 scale-0 group-hover:scale-100 transition-all duration-200 bg-gray-950 text-white text-xs font-bold px-3 py-1.5 rounded-lg shadow-xl whitespace-nowrap hidden md:block">
                    Chat with us!
                </span>
            </a>

            <Footer />
        </div>
    );
}

export default App;
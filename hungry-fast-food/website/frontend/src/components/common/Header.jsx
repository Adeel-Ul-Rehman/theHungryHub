// E:\hungryHub\hungry-fast-food\website\frontend\src\components\common\Header.jsx

import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { useCart } from '../../contexts/CartContext';
import { FaShoppingBag, FaBars, FaTimes, FaChevronRight, FaPhoneAlt, FaEnvelope } from 'react-icons/fa';
import ConfirmationModal from './ConfirmationModal';

export default function Header() {
    const { user, logout, isAuthenticated } = useAuth();
    const { cartCount, isRestaurantOpen, closedMessage } = useCart();
    const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
    const [showLogoutModal, setShowLogoutModal] = useState(false);
    const navigate = useNavigate();

    const handleConfirmLogout = async () => {
        setShowLogoutModal(false);
        await logout();
        navigate('/');
    };

    return (
        <>
            {!isRestaurantOpen && (
                <div className="bg-red-500 text-white font-bold text-sm py-2 marquee-container">
                    <div className="animate-marquee">
                        {closedMessage || "The restaurant is currently closed. Order taking is disabled."}
                    </div>
                </div>
            )}
            <header className="sticky top-0 z-40 bg-white shadow-md border-b border-gray-100">
            <div className="container mx-auto px-4 max-w-7xl">
                <div className="flex items-center justify-between h-16 md:h-20">
                    {/* Logo */}
                    <Link to="/" className="flex items-center gap-2.5 group">
                        <img src="/logo.png" alt="HungryHub Logo" className="h-10 md:h-12 w-auto group-hover:scale-105 transition-transform duration-300 object-contain" />
                        <span className="font-heading font-extrabold text-xl md:text-2xl tracking-tight text-primary">
                            Hungry<span className="text-text-primary">Hub</span>
                        </span>
                    </Link>

                    {/* Desktop Navigation */}
                    <nav className="hidden md:flex items-center gap-8 font-semibold text-text-primary">
                        <Link to="/" className="hover:text-primary transition-colors duration-200">Home</Link>
                        <Link to="/menu" className="hover:text-primary transition-colors duration-200">Menu</Link>
                        <Link to="/about" className="hover:text-primary transition-colors duration-200">About Us</Link>
                        <Link to="/contact" className="hover:text-primary transition-colors duration-200">Contact Us</Link>
                        {isAuthenticated && (
                            <Link to="/orders" className="hover:text-primary transition-colors duration-200">Order History</Link>
                        )}
                    </nav>

                    {/* Right side options */}
                    <div className="flex items-center gap-4">
                        {/* Cart Button */}
                        <Link to="/cart" className="relative p-2.5 bg-gray-50 rounded-full hover:bg-orange-50 text-text-primary hover:text-primary transition-all duration-300 group inline-flex items-center justify-center">
                            <FaShoppingBag className="w-5 h-5" />
                            {cartCount > 0 && (
                                <span className="absolute -top-1 -right-1 bg-primary text-white text-xs font-bold w-5 h-5 flex items-center justify-center rounded-full border-2 border-white animate-bounce-slow">
                                    {cartCount}
                                </span>
                            )}
                        </Link>

                        {/* User Profile / Auth links */}
                        <div className="hidden md:flex items-center gap-4 border-l border-gray-200 pl-4">
                            {isAuthenticated ? (
                                <div className="flex items-center gap-3">
                                    <div className="text-right">
                                        <p className="text-sm font-bold text-text-primary">{user?.full_name}</p>
                                        <p className="text-xs text-text-secondary">Customer</p>
                                    </div>
                                    <button
                                        onClick={() => setShowLogoutModal(true)}
                                        className="btn-outline px-4 py-2 text-sm"
                                    >
                                        Logout
                                    </button>
                                </div>
                            ) : (
                                <div className="flex items-center gap-3">
                                    <Link to="/login" className="text-text-primary font-bold hover:text-primary transition-colors">Sign In</Link>
                                    <Link to="/signup" className="btn-primary px-5 py-2.5 text-sm">Register</Link>
                                </div>
                            )}
                        </div>

                        {/* Mobile Menu Button */}
                        <button
                            onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
                            className="md:hidden p-2 text-text-primary hover:text-primary focus:outline-none rounded-full hover:bg-gray-50 transition-colors inline-flex items-center justify-center"
                        >
                            {mobileMenuOpen ? <FaTimes className="w-5 h-5" /> : <FaBars className="w-5 h-5" />}
                        </button>
                    </div>
                </div>
            </div>

            {/* Mobile Menu Backdrop */}
            {mobileMenuOpen && (
                <div 
                    className="md:hidden fixed inset-0 z-40 bg-black/50 transition-opacity duration-300 animate-fade-in"
                    onClick={() => setMobileMenuOpen(false)}
                />
            )}

            {/* Mobile Menu Drawer (Right to Left) */}
            {mobileMenuOpen && (
                <div className="md:hidden fixed top-0 right-0 bottom-0 h-screen w-72 max-w-[85vw] bg-white z-50 shadow-2xl p-6 flex flex-col justify-between animate-slide-left overflow-y-auto">
                    <div>
                        {/* Drawer Header */}
                        <div className="flex justify-between items-center pb-4 mb-6 border-b border-gray-100">
                            <span className="font-heading font-extrabold text-xl tracking-tight text-primary">
                                Hungry<span className="text-text-primary">Hub</span>
                            </span>
                            <button
                                onClick={() => setMobileMenuOpen(false)}
                                className="p-2 text-text-primary hover:text-primary transition-colors focus:outline-none rounded-full bg-gray-50 hover:bg-gray-100 inline-flex items-center justify-center"
                            >
                                <FaTimes className="w-4 h-4" />
                            </button>
                        </div>

                        {/* Navigation Links */}
                        <nav className="flex flex-col gap-4 font-semibold text-text-primary">
                            <Link to="/" onClick={() => setMobileMenuOpen(false)} className="hover:text-primary py-1.5 border-b border-gray-50 flex items-center justify-between">
                                <span>Home</span>
                                <FaChevronRight className="w-3 h-3 text-gray-400 shrink-0" />
                            </Link>
                            <Link to="/menu" onClick={() => setMobileMenuOpen(false)} className="hover:text-primary py-1.5 border-b border-gray-50 flex items-center justify-between">
                                <span>Menu</span>
                                <FaChevronRight className="w-3 h-3 text-gray-400 shrink-0" />
                            </Link>
                            <Link to="/about" onClick={() => setMobileMenuOpen(false)} className="hover:text-primary py-1.5 border-b border-gray-50 flex items-center justify-between">
                                <span>About Us</span>
                                <FaChevronRight className="w-3 h-3 text-gray-400 shrink-0" />
                            </Link>
                            <Link to="/contact" onClick={() => setMobileMenuOpen(false)} className="hover:text-primary py-1.5 border-b border-gray-50 flex items-center justify-between">
                                <span>Contact Us</span>
                                <FaChevronRight className="w-3 h-3 text-gray-400 shrink-0" />
                            </Link>
                            {isAuthenticated && (
                                <Link to="/orders" onClick={() => setMobileMenuOpen(false)} className="hover:text-primary py-1.5 border-b border-gray-50 flex items-center justify-between">
                                    <span>Order History</span>
                                    <FaChevronRight className="w-3 h-3 text-gray-400 shrink-0" />
                                </Link>
                            )}

                            {/* User options */}
                            <div className="pt-4 mt-2">
                                {isAuthenticated ? (
                                    <div className="flex flex-col gap-4">
                                        <div className="bg-gray-50 p-3.5 rounded-xl border border-gray-100">
                                            <p className="text-xs text-text-secondary font-bold uppercase tracking-wider">Logged In As</p>
                                            <p className="text-sm font-bold text-text-primary mt-1">{user?.full_name}</p>
                                            <p className="text-xs text-text-secondary truncate mt-0.5">{user?.email}</p>
                                        </div>
                                        <button
                                            onClick={() => {
                                                setMobileMenuOpen(false);
                                                setShowLogoutModal(true);
                                            }}
                                            className="btn-outline w-full py-2.5 text-center text-sm font-bold animate-pulse-slow"
                                        >
                                            Logout
                                        </button>
                                    </div>
                                ) : (
                                    <div className="flex flex-col gap-3">
                                        <Link to="/login" onClick={() => setMobileMenuOpen(false)} className="btn-outline w-full py-2.5 text-center text-sm font-bold">Sign In</Link>
                                        <Link to="/signup" onClick={() => setMobileMenuOpen(false)} className="btn-primary w-full py-2.5 text-center text-sm font-bold">Register</Link>
                                    </div>
                                )}
                            </div>
                        </nav>
                    </div>

                    {/* Quick Info Drawer Footer */}
                    <div className="mt-8 border-t border-gray-100 pt-6 space-y-3">
                        <div className="text-xs text-text-secondary space-y-2">
                            <p className="flex items-center gap-2">
                                <FaPhoneAlt className="text-primary shrink-0" />
                                <span>0336-0357333</span>
                            </p>
                            <p className="flex items-center gap-2">
                                <FaEnvelope className="text-primary shrink-0" />
                                <span className="truncate">thehungryhub26@gmail.com</span>
                            </p>
                        </div>
                    </div>
                </div>
            )}
            {/* Custom Logout Confirmation Modal */}
            <ConfirmationModal
                isOpen={showLogoutModal}
                title="Log Out Confirmation"
                message="Are you sure you want to log out of your HungryHub account?"
                confirmText="Log Out"
                cancelText="Cancel"
                onConfirm={handleConfirmLogout}
                onCancel={() => setShowLogoutModal(false)}
                type="danger"
            />
            </header>
        </>
    );
}

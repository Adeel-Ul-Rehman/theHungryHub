import React from 'react';
import { Link } from 'react-router-dom';
import { FaTiktok, FaInstagram, FaWhatsapp, FaEnvelope, FaFileAlt, FaMapMarkerAlt, FaPhoneAlt } from 'react-icons/fa';

export default function Footer() {
    return (
        <footer className="bg-gradient-to-br from-secondary to-gray-950 text-white pt-12 pb-8 border-t-4 border-primary">
            <div className="container mx-auto px-4 max-w-7xl">
                <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-8 mb-8">
                    {/* Brand & Social Media Column */}
                    <div className="flex flex-col gap-4">
                        <Link to="/" className="flex items-center gap-2.5">
                            <img src="/logo.png" alt="HungryHub Logo" className="h-8 w-auto object-contain" />
                            <span className="font-heading font-extrabold text-xl tracking-tight text-primary">
                                Hungry<span className="text-white">Hub</span>
                            </span>
                        </Link>
                        <p className="text-gray-300 text-sm leading-relaxed">
                            Craving delicious fast food? HungryHub serves the tastiest burgers, pizzas, and deals hot and fresh right to your doorstep.
                        </p>
                        
                        {/* Social Media Section */}
                        <div className="flex flex-col gap-2 mt-2">
                            <h4 className="font-heading font-bold text-sm text-secondary uppercase tracking-wider">Social Media</h4>
                            <div className="flex gap-3">
                                <a
                                    href="https://www.tiktok.com/@thehungryhub26?_r=1&_t=ZS-97ySctfx8lw"
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    className="p-2.5 bg-gray-900 rounded-full hover:bg-primary transition-all duration-300 hover:scale-105 inline-flex items-center justify-center text-white"
                                    title="TikTok"
                                >
                                    <FaTiktok className="w-4 h-4 shrink-0" />
                                </a>
                                <a
                                    href="https://instagram.com/thehungryhub26"
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    className="p-2.5 bg-gray-900 rounded-full hover:bg-primary transition-all duration-300 hover:scale-105 inline-flex items-center justify-center text-white"
                                    title="Instagram"
                                >
                                    <FaInstagram className="w-4 h-4 shrink-0" />
                                </a>
                                <a
                                    href="https://wa.me/923391191147"
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    className="p-2.5 bg-gray-900 rounded-full hover:bg-primary transition-all duration-300 hover:scale-105 inline-flex items-center justify-center text-white"
                                    title="WhatsApp"
                                >
                                    <FaWhatsapp className="w-4 h-4 shrink-0" />
                                </a>
                                <a
                                    href="mailto:thehungryhub26@gmail.com"
                                    className="p-2.5 bg-gray-900 rounded-full hover:bg-primary transition-all duration-300 hover:scale-105 inline-flex items-center justify-center text-white"
                                    title="Email"
                                >
                                    <FaEnvelope className="w-4 h-4 shrink-0" />
                                </a>
                            </div>
                        </div>
                    </div>

                    {/* Quick Links Column */}
                    <div>
                        <h4 className="font-heading font-bold text-lg mb-4 text-secondary">Quick Links</h4>
                        <ul className="flex flex-col gap-2.5 text-gray-300 text-sm font-semibold">
                            <li><Link to="/" className="hover:text-primary transition-colors">Home</Link></li>
                            <li><Link to="/menu" className="hover:text-primary transition-colors">Menu</Link></li>
                            <li><Link to="/menu/deals" className="hover:text-primary transition-colors">Deals</Link></li>
                            <li><Link to="/about" className="hover:text-primary transition-colors">About Us</Link></li>
                            <li><Link to="/contact" className="hover:text-primary transition-colors">Contact Us</Link></li>
                        </ul>
                    </div>

                    {/* Support Column */}
                    <div>
                        <h4 className="font-heading font-bold text-lg mb-4 text-secondary">Support</h4>
                        <ul className="flex flex-col gap-2.5 text-gray-300 text-sm font-semibold">
                            <li><Link to="/contact" className="hover:text-primary transition-colors">Contact Us</Link></li>
                            <li><Link to="/refund-policy" className="hover:text-primary transition-colors">Refund Policy</Link></li>
                            <li>
                                <a 
                                    href="https://drive.google.com/file/d/1u2pLR3H6bc0DGxTzxYTM-9lnTgUJT4Zq/view?usp=drive_link" 
                                    target="_blank" 
                                    rel="noopener noreferrer" 
                                    className="hover:text-primary transition-colors inline-flex items-center gap-1.5"
                                >
                                    <FaFileAlt className="w-3.5 h-3.5 shrink-0" />
                                    Terms of Service
                                </a>
                            </li>
                        </ul>
                    </div>

                    {/* Contact Column */}
                    <div>
                        <h4 className="font-heading font-bold text-lg mb-4 text-secondary">Contact Info</h4>
                        <ul className="flex flex-col gap-3 text-gray-300 text-[13px] sm:text-sm">
                            <li className="flex items-start gap-2.5">
                                <FaMapMarkerAlt className="text-primary mt-1 shrink-0" />
                                <a
                                    href="https://maps.app.goo.gl/34HTbGLe8NDxXcKBA"
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    className="hover:text-primary transition-colors text-white text-[12px] sm:text-[13px] md:text-sm"
                                >
                                    Zaki Plaza Muslim Town, Rawalpindi
                                </a>
                            </li>
                            <li className="flex items-center gap-2.5">
                                <FaPhoneAlt className="text-primary shrink-0" />
                                <a href="tel:03360357333" className="hover:text-primary transition-colors text-white whitespace-nowrap text-[12px] sm:text-[13px] md:text-sm">
                                    0336-0357333
                                </a>
                            </li>
                            <li className="flex items-center gap-2.5">
                                <FaWhatsapp className="text-[#25D366] shrink-0" />
                                <a href="https://wa.me/923391191147" target="_blank" rel="noopener noreferrer" className="hover:text-primary transition-colors text-white font-semibold whitespace-nowrap text-[12px] sm:text-[13px] md:text-sm">
                                    +92 3391191147
                                </a>
                            </li>
                            <li className="flex items-center gap-2.5">
                                <FaEnvelope className="text-primary shrink-0" />
                                <a href="mailto:thehungryhub26@gmail.com" className="hover:text-primary transition-colors text-white break-all text-[11px] sm:text-[12px] md:text-sm">
                                    thehungryhub26@gmail.com
                                </a>
                            </li>
                        </ul>
                    </div>
                </div>

                <hr className="border-gray-800 my-6" />

                <div className="flex flex-col md:flex-row items-center justify-between gap-4 text-xs text-gray-400 font-semibold">
                    <p>&copy; {new Date().getFullYear()} HungryHub. All rights reserved.</p>
                    <div className="flex gap-4">
                        <span className="hover:text-white cursor-pointer">Privacy Policy</span>
                        <span>&middot;</span>
                        <a 
                            href="https://drive.google.com/file/d/1u2pLR3H6bc0DGxTzxYTM-9lnTgUJT4Zq/view?usp=drive_link" 
                            target="_blank" 
                            rel="noopener noreferrer" 
                            className="hover:text-white transition-colors"
                        >
                            Terms of Service
                        </a>
                        <span>&middot;</span>
                        <Link to="/refund-policy" className="hover:text-white transition-colors">Refund Policy</Link>
                    </div>
                </div>
            </div>
        </footer>
    );
}

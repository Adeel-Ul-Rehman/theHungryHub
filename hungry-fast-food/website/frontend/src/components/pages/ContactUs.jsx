import React, { useState } from 'react';
import ToastNotification from '../common/ToastNotification';
import { FaMapMarkerAlt, FaPhoneAlt, FaWhatsapp, FaEnvelope } from 'react-icons/fa';

export default function ContactUs() {
    const [name, setName] = useState('');
    const [email, setEmail] = useState('');
    const [phone, setPhone] = useState('');
    const [subject, setSubject] = useState('General Inquiry');
    const [message, setMessage] = useState('');

    const [loading, setLoading] = useState(false);
    const [toast, setToast] = useState(null);

    const handleSubmit = async (e) => {
        e.preventDefault();

        // Basic validation
        if (!name.trim() || !email.trim() || !message.trim()) {
            setToast({ type: 'error', message: 'Please fill in all required fields.' });
            return;
        }

        setLoading(true);

        // Simulate API call
        setTimeout(() => {
            setLoading(false);
            setToast({ type: 'success', message: 'Message sent successfully! We will get back to you soon.' });

            // Clear form
            setName('');
            setEmail('');
            setPhone('');
            setSubject('General Inquiry');
            setMessage('');
        }, 1500);
    };

    return (
        <div className="space-y-16 py-4 animate-slide-up">
            {/* Hero Header */}
            <section className="relative rounded-3xl bg-gradient-to-br from-secondary to-gray-950 overflow-hidden shadow-xl text-white py-16 px-6 md:px-12 md:py-24 text-center">
                <div className="absolute inset-0 bg-black opacity-10"></div>
                <div className="relative z-10 max-w-3xl mx-auto space-y-6">
                    <span className="bg-primary text-white text-xs font-extrabold uppercase px-4 py-1.5 rounded-full tracking-wider shadow-sm animate-pulse">
                        Get In Touch
                    </span>
                    <h1 className="font-heading font-black text-3xl sm:text-4xl md:text-6xl leading-tight">
                        We'd Love To <br />
                        <span className="text-primary">Hear From You</span>
                    </h1>
                    <p className="text-gray-300 text-sm md:text-lg leading-relaxed max-w-xl mx-auto">
                        Have a question, feedback, or need help with an order? Drop us a line!
                    </p>
                </div>
            </section>

            {/* Layout Grid */}
            <section className="grid grid-cols-1 lg:grid-cols-5 gap-12 items-start">
                {/* Contact Information Cards */}
                <div className="lg:col-span-2 space-y-6">
                    <div className="bg-white border border-gray-100 p-6 md:p-8 rounded-2xl shadow-md space-y-6">
                        <h2 className="font-heading font-extrabold text-2xl text-text-primary">
                            Contact Information
                        </h2>

                        <div className="space-y-4 text-[13px] sm:text-sm">
                            {/* Address */}
                            <div className="flex items-start gap-3 sm:gap-4">
                                <span className="p-2 sm:p-3 bg-orange-50 rounded-xl text-primary text-base sm:text-lg flex items-center justify-center shrink-0">
                                    <FaMapMarkerAlt />
                                </span>
                                <div>
                                    <h4 className="font-bold text-text-primary text-xs sm:text-sm">Our Location</h4>
                                    <a
                                        href="https://maps.app.goo.gl/34HTbGLe8NDxXcKBA"
                                        target="_blank"
                                        rel="noopener noreferrer"
                                        className="text-text-secondary hover:text-primary transition-colors mt-0.5 block text-[11px] sm:text-[13px] md:text-sm"
                                    >
                                        Zaki Plaza, Muslim Town, Rawalpindi, Pakistan
                                    </a>
                                </div>
                            </div>

                            {/* Phone */}
                            <div className="flex items-start gap-3 sm:gap-4">
                                <span className="p-2 sm:p-3 bg-orange-50 rounded-xl text-primary text-base sm:text-lg flex items-center justify-center shrink-0">
                                    <FaPhoneAlt />
                                </span>
                                <div>
                                    <h4 className="font-bold text-text-primary text-xs sm:text-sm">Phone Number</h4>
                                    <a href="tel:03360357333" className="text-text-secondary hover:text-primary transition-colors mt-0.5 block whitespace-nowrap text-[11px] sm:text-[13px] md:text-sm">
                                        0336-0357333
                                    </a>
                                </div>
                            </div>

                            {/* WhatsApp */}
                            <div className="flex items-start gap-3 sm:gap-4">
                                <span className="p-2 sm:p-3 bg-orange-50 rounded-xl text-primary text-base sm:text-lg flex items-center justify-center shrink-0">
                                    <FaWhatsapp />
                                </span>
                                <div>
                                    <h4 className="font-bold text-text-primary text-xs sm:text-sm">WhatsApp</h4>
                                    <a
                                        href="https://wa.me/923391191147"
                                        target="_blank"
                                        rel="noopener noreferrer"
                                        className="text-text-secondary hover:text-primary transition-colors mt-0.5 block font-semibold text-green-600 whitespace-nowrap text-[11px] sm:text-[13px] md:text-sm"
                                    >
                                        +92 3391191147 (Chat Now)
                                    </a>
                                </div>
                            </div>

                            {/* Email */}
                            <div className="flex items-start gap-3 sm:gap-4">
                                <span className="p-2 sm:p-3 bg-orange-50 rounded-xl text-primary text-base sm:text-lg flex items-center justify-center shrink-0">
                                    <FaEnvelope />
                                </span>
                                <div>
                                    <h4 className="font-bold text-text-primary text-xs sm:text-sm">Email Support</h4>
                                    <a href="mailto:thehungryhub26@gmail.com" className="text-text-secondary hover:text-primary transition-colors mt-0.5 block break-all text-[11px] sm:text-[12px] md:text-sm">
                                        thehungryhub26@gmail.com
                                    </a>
                                </div>
                            </div>
                        </div>
                    </div>

                    {/* Operational Details Card */}
                    <div className="bg-gradient-to-br from-secondary to-gray-950 text-white p-6 md:p-8 rounded-2xl shadow-md space-y-6">
                        <div className="space-y-4">
                            <h3 className="font-heading font-bold text-xl text-primary">Hours & Orders</h3>

                            <div className="space-y-2 text-xs sm:text-sm">
                                <div className="flex justify-between items-center gap-2 border-b border-gray-800 pb-1.5">
                                    <span className="text-gray-300 shrink-0">Working Hours:</span>
                                    <span className="font-semibold text-white text-right whitespace-nowrap">
                                        Mon-Sun: 11:00 AM - 11:00 PM
                                    </span>
                                </div>

                                <div className="flex justify-between items-start gap-2 border-b border-gray-800 pb-1.5">
                                    <span className="text-gray-300 shrink-0">Order Confirmation:</span>
                                    <span className="font-semibold text-white text-right leading-tight max-w-[60%]">
                                        We call to confirm all orders
                                    </span>
                                </div>
                            </div>
                        </div>

                        {/* Styled WhatsApp floating-action helper */}
                        <a
                            href="https://wa.me/923391191147"
                            target="_blank"
                            rel="noopener noreferrer"
                            className="w-full inline-flex items-center justify-center gap-2 whitespace-nowrap bg-green-600 hover:bg-green-700 text-white text-sm sm:text-base font-bold py-3 px-3 sm:px-4 rounded-xl shadow-lg hover:scale-[1.02] transition-transform"
                        >
                            <FaWhatsapp className="w-4 h-4 sm:w-5 sm:h-5 fill-current shrink-0" />
                            <span className="whitespace-nowrap">
                                WhatsApp Customer Support
                            </span>
                        </a>
                    </div>
                </div>

                {/* Contact Form & Map */}
                <div className="lg:col-span-3 space-y-6">
                    <div className="bg-white border border-gray-100 p-6 md:p-8 rounded-2xl shadow-md">
                        <h2 className="font-heading font-extrabold text-2xl text-text-primary mb-6">
                            Send Us a Message
                        </h2>

                        <form onSubmit={handleSubmit} className="space-y-4">
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                {/* Name */}
                                <div className="space-y-1.5">
                                    <label className="text-xs font-bold text-text-secondary uppercase tracking-wider">Full Name *</label>
                                    <input
                                        type="text"
                                        value={name}
                                        onChange={(e) => setName(e.target.value)}
                                        className="input-field"
                                        placeholder="John Doe"
                                        required
                                    />
                                </div>

                                {/* Email */}
                                <div className="space-y-1.5">
                                    <label className="text-xs font-bold text-text-secondary uppercase tracking-wider">Email Address *</label>
                                    <input
                                        type="email"
                                        value={email}
                                        onChange={(e) => setEmail(e.target.value)}
                                        className="input-field"
                                        placeholder="johndoe@example.com"
                                        required
                                    />
                                </div>
                            </div>

                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                {/* Phone (Optional) */}
                                <div className="space-y-1.5">
                                    <label className="text-xs font-bold text-text-secondary uppercase tracking-wider">Phone Number (Optional)</label>
                                    <input
                                        type="tel"
                                        value={phone}
                                        onChange={(e) => setPhone(e.target.value)}
                                        className="input-field"
                                        placeholder="0300-1234567"
                                    />
                                </div>

                                {/* Subject */}
                                <div className="space-y-1.5">
                                    <label className="text-xs font-bold text-text-secondary uppercase tracking-wider">Subject *</label>
                                    <select
                                        value={subject}
                                        onChange={(e) => setSubject(e.target.value)}
                                        className="input-field bg-white"
                                    >
                                        <option value="General Inquiry">General Inquiry</option>
                                        <option value="Order Issue">Order Issue</option>
                                        <option value="Feedback">Feedback</option>
                                        <option value="Other">Other</option>
                                    </select>
                                </div>
                            </div>

                            {/* Message */}
                            <div className="space-y-1.5">
                                <label className="text-xs font-bold text-text-secondary uppercase tracking-wider">Message *</label>
                                <textarea
                                    value={message}
                                    onChange={(e) => setMessage(e.target.value)}
                                    rows={5}
                                    className="input-field resize-none"
                                    placeholder="Write your message details here..."
                                    required
                                ></textarea>
                            </div>

                            {/* Submit */}
                            <button
                                type="submit"
                                disabled={loading}
                                className="btn-primary w-full md:w-auto font-bold shadow-lg shadow-orange-100/50 flex items-center justify-center gap-2"
                            >
                                {loading ? (
                                    <>
                                        <svg className="animate-spin h-5 w-5 text-white" viewBox="0 0 24 24">
                                            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" fill="none" />
                                            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                                        </svg>
                                        Sending Message...
                                    </>
                                ) : (
                                    'Send Message'
                                )}
                            </button>
                        </form>
                    </div>

                    {/* Google Maps Embed Location */}
                    <div className="relative rounded-2xl overflow-hidden border border-gray-100 shadow-md min-h-[250px]">
                        <iframe
                            title="Hungry Hub Branch Map"
                            src="https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3322.911364506246!2d73.0768407!3d33.6420516!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x38df952136e09c85%3A0xe54d3f572a15998a!2sZaki%20Plaza%2C%20Muslim%20Town%2C%20Rawalpindi!5e0!3m2!1sen!2spk!4v1700000000000!5m2!1sen!2spk"
                            className="absolute inset-0 w-full h-full border-0"
                            allowFullScreen=""
                            loading="lazy"
                            referrerPolicy="no-referrer-when-downgrade"
                        ></iframe>
                    </div>
                </div>
            </section>

            {/* Local Toast Alert */}
            {toast && (
                <ToastNotification
                    message={toast.message}
                    type={toast.type}
                    onClose={() => setToast(null)}
                />
            )}
        </div>
    );
}

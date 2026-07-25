import React from 'react';
import { Link } from 'react-router-dom';
import { FaStar, FaLeaf, FaHeart, FaBolt, FaMapMarkerAlt, FaPhoneAlt, FaEnvelope } from 'react-icons/fa';

export default function AboutUs() {
    return (
        <div className="space-y-16 py-4 animate-slide-up">
            {/* Hero Section */}
            <section className="relative rounded-3xl bg-gradient-to-br from-secondary to-gray-950 overflow-hidden shadow-xl text-white py-16 px-6 md:px-12 md:py-24 text-center">
                <div className="absolute inset-0 bg-black opacity-10"></div>
                <div className="relative z-10 max-w-3xl mx-auto space-y-6">
                    <span className="bg-primary text-white text-xs font-extrabold uppercase px-4 py-1.5 rounded-full tracking-wider shadow-sm animate-pulse">
                        About Hungry Hub
                    </span>
                    <h1 className="font-heading font-black text-3xl sm:text-4xl md:text-6xl leading-tight">
                        Serving Happiness <br />
                        <span className="text-primary">One Bite At A Time</span>
                    </h1>
                    <p className="text-gray-300 text-sm md:text-lg leading-relaxed max-w-xl mx-auto">
                        Discover the story behind Rawalpindi's most loved fast food destination.
                    </p>
                </div>
            </section>

            {/* Our Story Section */}
            <section className="grid grid-cols-1 md:grid-cols-2 gap-12 items-center">
                <div className="space-y-6">
                    <h2 className="font-heading font-extrabold text-2xl sm:text-3xl text-text-primary relative inline-block">
                        Our Story
                        <span className="absolute bottom-0 left-0 w-16 h-1 bg-primary rounded-full"></span>
                    </h2>
                    <p className="text-text-secondary text-sm md:text-base leading-relaxed">
                        Hungry Hub is your go-to destination for delicious fast food in Rawalpindi. We specialize in burgers, pizzas, and mouth-watering deals that bring people together. Our commitment to quality and customer satisfaction makes us the preferred choice for food lovers.
                    </p>
                    <p className="text-text-secondary text-sm md:text-base leading-relaxed">
                        Led by our dedicated manager Rehan Khan, our team works tirelessly to ensure every order meets our high standards of taste and presentation. We believe in creating memorable dining experiences, whether you visit us in person or order online.
                    </p>
                </div>
                <div className="relative rounded-2xl overflow-hidden shadow-xl border border-gray-150 h-72 md:h-96 bg-gray-100 group">
                    <img 
                        src="/branch.png" 
                        alt="Hungry Hub Branch" 
                        className="w-full h-full object-cover transform group-hover:scale-105 transition-transform duration-500"
                    />
                    <div className="absolute inset-0 bg-gradient-to-t from-black/85 via-black/30 to-transparent z-10"></div>
                    <div className="absolute bottom-6 left-6 z-20 space-y-2 text-white">
                        <span className="bg-secondary text-white text-xs uppercase font-extrabold px-3 py-1.5 rounded-full shadow-sm">
                            Restaurant Branch
                        </span>
                        <h4 className="font-heading font-bold text-lg">Muslim Town, Rawalpindi</h4>
                        <p className="text-gray-300 text-xs">Drop by for a premium dining experience</p>
                    </div>
                </div>
            </section>

            {/* Our Values Section */}
            <section className="space-y-8 bg-gray-50 rounded-3xl p-8 md:p-12 border border-gray-100">
                <div className="text-center space-y-2">
                    <h2 className="font-heading font-extrabold text-2xl sm:text-3xl text-text-primary">
                        Our Core Values
                    </h2>
                    <p className="text-text-secondary text-sm max-w-md mx-auto">
                        The principles that guide us every single day to serve you the absolute best.
                    </p>
                </div>
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
                    <div className="card bg-white p-6 text-center border border-gray-100 space-y-4">
                        <div className="w-12 h-12 bg-orange-50 text-2xl flex items-center justify-center rounded-full mx-auto text-primary">
                            <FaStar className="w-5 h-5 shrink-0" />
                        </div>
                        <h3 className="font-bold text-lg text-text-primary">Quality</h3>
                        <p className="text-text-secondary text-xs leading-relaxed">
                            We never compromise on the quality of our food, using premium selected cuts and ingredients.
                        </p>
                    </div>
                    <div className="card bg-white p-6 text-center border border-gray-100 space-y-4">
                        <div className="w-12 h-12 bg-orange-50 text-2xl flex items-center justify-center rounded-full mx-auto text-primary">
                            <FaLeaf className="w-5 h-5 shrink-0" />
                        </div>
                        <h3 className="font-bold text-lg text-text-primary">Freshness</h3>
                        <p className="text-text-secondary text-xs leading-relaxed">
                            Every meal is made-to-order, ensuring maximum freshness and optimal taste.
                        </p>
                    </div>
                    <div className="card bg-white p-6 text-center border border-gray-100 space-y-4">
                        <div className="w-12 h-12 bg-orange-50 text-2xl flex items-center justify-center rounded-full mx-auto text-primary">
                            <FaHeart className="w-5 h-5 shrink-0" />
                        </div>
                        <h3 className="font-bold text-lg text-text-primary">Customer Satisfaction</h3>
                        <p className="text-text-secondary text-xs leading-relaxed">
                            Our customers are our family. Your satisfaction is our primary metric of success.
                        </p>
                    </div>
                    <div className="card bg-white p-6 text-center border border-gray-100 space-y-4">
                        <div className="w-12 h-12 bg-orange-50 text-2xl flex items-center justify-center rounded-full mx-auto text-primary">
                            <FaBolt className="w-5 h-5 shrink-0" />
                        </div>
                        <h3 className="font-bold text-lg text-text-primary">Fast Delivery</h3>
                        <p className="text-text-secondary text-xs leading-relaxed">
                            Piping hot food delivered straight to your door in 30 minutes or less.
                        </p>
                    </div>
                </div>
            </section>

            {/* Our Team Section */}
            <section className="grid grid-cols-1 md:grid-cols-2 gap-12 items-center">
                <div className="order-2 md:order-1 relative rounded-2xl overflow-hidden shadow-xl border border-gray-150 h-72 md:h-96 bg-gray-100 group">
                    <img 
                        src="/manager.png" 
                        alt="Restaurant Manager - Rehan Khan" 
                        className="w-full h-full object-cover transform group-hover:scale-105 transition-transform duration-500"
                    />
                    <div className="absolute inset-0 bg-gradient-to-t from-black/85 via-black/30 to-transparent z-10"></div>
                    <div className="absolute bottom-6 left-6 z-20 space-y-2 text-white">
                        <span className="bg-primary text-white text-xs uppercase font-extrabold px-3 py-1.5 rounded-full shadow-sm">
                            Restaurant Manager
                        </span>
                        <h4 className="font-heading font-bold text-lg">Rehan Khan</h4>
                        <p className="text-gray-300 text-xs">Hungry Hub General Manager</p>
                    </div>
                </div>
                <div className="order-1 md:order-2 space-y-6">
                    <h2 className="font-heading font-extrabold text-2xl sm:text-3xl text-text-primary relative inline-block">
                        Meet Our Manager
                        <span className="absolute bottom-0 left-0 w-16 h-1 bg-primary rounded-full"></span>
                    </h2>
                    <h3 className="text-xl font-bold text-text-primary">Rehan Khan</h3>
                    <p className="text-text-secondary text-sm md:text-base leading-relaxed">
                        Under the energetic leadership of Rehan Khan, Hungry Hub has grown to be a staple food choice in Rawalpindi. With over a decade of hospitality experience, Rehan ensures the kitchen operates with strict hygiene standards and that customer service remains friendly and efficient.
                    </p>
                    <p className="text-text-secondary text-sm md:text-base leading-relaxed">
                        "We believe in food that doesn't just fill your stomach but fills your heart. Every single burger we press and pizza we bake represents our passion for delicious experiences."
                    </p>
                </div>
            </section>

            {/* Visit Us Section */}
            <section className="space-y-8">
                <div className="text-center space-y-2">
                    <h2 className="font-heading font-extrabold text-2xl sm:text-3xl text-text-primary">
                        Visit Our Branch
                    </h2>
                    <p className="text-text-secondary text-sm max-w-md mx-auto">
                        Come dine with us or get your takeaway fresh from the counter.
                    </p>
                </div>
                <div className="grid grid-cols-1 md:grid-cols-3 gap-8 items-stretch">
                    {/* Branch Details */}
                    <div className="bg-white border border-gray-100 p-6 md:p-8 rounded-2xl shadow-md space-y-6 flex flex-col justify-between">
                        <div className="space-y-4">
                            <h3 className="font-heading font-bold text-lg md:text-xl text-text-primary">Rawalpindi Branch</h3>
                            <div className="space-y-3 text-[13px] sm:text-sm text-text-secondary">
                                <p className="flex items-start gap-2.5">
                                    <FaMapMarkerAlt className="text-primary mt-1 shrink-0" />
                                    <span className="text-[12px] sm:text-[13px] md:text-sm">Zaki Plaza, Muslim Town, Rawalpindi, Pakistan</span>
                                </p>
                                <p className="flex items-center gap-2.5">
                                    <FaPhoneAlt className="text-primary shrink-0" />
                                    <span className="whitespace-nowrap text-[12px] sm:text-[13px] md:text-sm">0336-0357333</span>
                                </p>
                                <p className="flex items-center gap-2.5">
                                    <FaEnvelope className="text-primary shrink-0" />
                                    <span className="break-all text-[11px] sm:text-[12px] md:text-sm">thehungryhub26@gmail.com</span>
                                </p>
                            </div>
                        </div>
                        <a
                            href="https://maps.app.goo.gl/34HTbGLe8NDxXcKBA"
                            target="_blank"
                            rel="noopener noreferrer"
                            className="btn-primary w-full text-center font-bold shadow-lg shadow-orange-100/50 block py-3"
                        >
                            Open in Google Maps
                        </a>
                    </div>

                    {/* Google Maps Embed Placeholder */}
                    <div className="md:col-span-2 relative rounded-2xl overflow-hidden border border-gray-100 shadow-md min-h-[300px]">
                        <iframe
                            title="Hungry Hub Location Map"
                            src="https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3322.911364506246!2d73.0768407!3d33.6420516!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x38df952136e09c85%3A0xe54d3f572a15998a!2sZaki%20Plaza%2C%20Muslim%20Town%2C%20Rawalpindi!5e0!3m2!1sen!2spk!4v1700000000000!5m2!1sen!2spk"
                            className="absolute inset-0 w-full h-full border-0"
                            allowFullScreen=""
                            loading="lazy"
                            referrerPolicy="no-referrer-when-downgrade"
                        ></iframe>
                    </div>
                </div>
            </section>
        </div>
    );
}

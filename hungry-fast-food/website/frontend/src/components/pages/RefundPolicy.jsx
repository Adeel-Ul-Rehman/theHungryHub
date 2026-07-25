// E:\hungryHub\hungry-fast-food\website\frontend\src\components\pages\RefundPolicy.jsx

import React from 'react';
import { 
    FaBox, 
    FaExclamationTriangle, 
    FaRegClock, 
    FaUtensils, 
    FaSearch, 
    FaCreditCard, 
    FaComments, 
    FaPhoneAlt, 
    FaHandshake, 
    FaPizzaSlice, 
    FaEnvelope 
} from 'react-icons/fa';

export default function RefundPolicy() {
    return (
        <div className="space-y-16 py-4 animate-slide-up">
            {/* Hero Section */}
            <section className="relative rounded-3xl bg-gradient-to-br from-[#264653] via-[#E63946] to-[#F4A261] overflow-hidden shadow-xl text-white py-16 px-6 md:px-12 md:py-24 text-center">
                <div className="absolute inset-0 bg-black opacity-15"></div>
                <div className="relative z-10 max-w-3xl mx-auto space-y-6">
                    <span className="bg-[#E63946] text-white text-xs font-extrabold uppercase px-4 py-1.5 rounded-full tracking-wider shadow-sm animate-pulse">
                        Hungry Hub Policies
                    </span>
                    <h1 className="font-heading font-black text-3xl sm:text-4xl md:text-6xl leading-tight">
                        Refund & <span className="text-[#F4A261]">Cancellation</span> Policy
                    </h1>
                    <p className="text-gray-150 text-sm md:text-lg leading-relaxed max-w-xl mx-auto font-medium">
                        We value your satisfaction and trust. Please read our guidelines on returns, cancellations, and refunds below.
                    </p>
                </div>
            </section>

            {/* Policy Content Grid */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                {/* Return Policy Card */}
                <section className="bg-white border border-gray-100 p-6 md:p-8 rounded-3xl shadow-md hover:shadow-lg transition-all duration-300 flex flex-col justify-between hover:scale-[1.01]">
                    <div className="space-y-6">
                        <div className="flex items-center gap-3">
                            <span className="p-3 bg-red-50 rounded-2xl text-2xl text-[#E63946]">
                                <FaBox />
                            </span>
                            <div>
                                <h2 className="font-heading font-extrabold text-2xl text-[#264653]">
                                    Return Policy
                                </h2>
                                <p className="text-gray-500 text-xs mt-0.5">When returns are accepted</p>
                            </div>
                        </div>
                        <p className="text-gray-600 text-sm leading-relaxed">
                            At Hungry Hub, we strive to deliver every meal hot and fresh. Returns are accepted exclusively under the following circumstances:
                        </p>

                        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 pt-2">
                            {/* Quality Compromised */}
                            <div className="flex items-start gap-3.5 p-4 rounded-2xl bg-red-50/40 border border-red-100/50">
                                <span className="text-xl p-2 bg-white rounded-xl shadow-sm inline-flex items-center justify-center text-[#E63946]">
                                    <FaExclamationTriangle />
                                </span>
                                <div>
                                    <h4 className="font-bold text-[#264653] text-sm">Quality Compromised</h4>
                                    <p className="text-gray-500 text-[11px] mt-0.5 leading-snug">Food quality is below standard or spoiled.</p>
                                </div>
                            </div>

                            {/* Delayed Delivery */}
                            <div className="flex items-start gap-3.5 p-4 rounded-2xl bg-orange-50/40 border border-orange-100/50">
                                <span className="text-xl p-2 bg-white rounded-xl shadow-sm inline-flex items-center justify-center text-[#F4A261]">
                                    <FaRegClock />
                                </span>
                                <div>
                                    <h4 className="font-bold text-[#264653] text-sm">Delayed Delivery</h4>
                                    <p className="text-gray-500 text-[11px] mt-0.5 leading-snug">Delivery time exceeds the 30-minute limit.</p>
                                </div>
                            </div>

                            {/* Incorrect Order */}
                            <div className="flex items-start gap-3.5 p-4 rounded-2xl bg-blue-50/40 border border-blue-100/50">
                                <span className="text-xl p-2 bg-white rounded-xl shadow-sm inline-flex items-center justify-center text-[#264653]">
                                    <FaUtensils />
                                </span>
                                <div>
                                    <h4 className="font-bold text-[#264653] text-sm">Incorrect Order</h4>
                                    <p className="text-gray-500 text-[11px] mt-0.5 leading-snug">Wrong items were delivered to you.</p>
                                </div>
                            </div>

                            {/* Missing Items */}
                            <div className="flex items-start gap-3.5 p-4 rounded-2xl bg-yellow-50/40 border border-yellow-100/50">
                                <span className="text-xl p-2 bg-white rounded-xl shadow-sm inline-flex items-center justify-center text-secondary">
                                    <FaSearch />
                                </span>
                                <div>
                                    <h4 className="font-bold text-[#264653] text-sm">Missing Items</h4>
                                    <p className="text-gray-500 text-[11px] mt-0.5 leading-snug">Items are missing from your order.</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </section>

                {/* Refund Process Card */}
                <section className="bg-white border border-gray-100 p-6 md:p-8 rounded-3xl shadow-md hover:shadow-lg transition-all duration-300 flex flex-col justify-between hover:scale-[1.01]">
                    <div className="space-y-6">
                        <div className="flex items-center gap-3">
                            <span className="p-3 bg-amber-50 rounded-2xl text-2xl text-[#F4A261]">
                                <FaCreditCard />
                            </span>
                            <div>
                                <h2 className="font-heading font-extrabold text-2xl text-[#264653]">
                                    Refund Process
                                </h2>
                                <p className="text-gray-500 text-xs mt-0.5">How your refund is handled</p>
                            </div>
                        </div>
                        <p className="text-gray-600 text-sm leading-relaxed">
                            Once a return or issue is confirmed by our customer support team, we process refunds promptly and transparently:
                        </p>

                        <div className="space-y-4 pt-2">
                            {/* Step 1 */}
                            <div className="flex gap-4">
                                <div className="flex flex-col items-center">
                                    <span className="w-8 h-8 rounded-full bg-[#E63946] text-white flex items-center justify-center font-bold text-xs shadow-sm">1</span>
                                    <div className="w-0.5 h-6 bg-gray-100"></div>
                                </div>
                                <div className="pt-0.5">
                                    <h4 className="font-bold text-[#264653] text-sm">Right to Inspect</h4>
                                    <p className="text-gray-500 text-xs mt-0.5">We reserve the right to inspect claims and food items before processing refunds.</p>
                                </div>
                            </div>

                            {/* Step 2 */}
                            <div className="flex gap-4">
                                <div className="flex flex-col items-center">
                                    <span className="w-8 h-8 rounded-full bg-[#F4A261] text-white flex items-center justify-center font-bold text-xs shadow-sm">2</span>
                                    <div className="w-0.5 h-6 bg-gray-100"></div>
                                </div>
                                <div className="pt-0.5">
                                    <h4 className="font-bold text-[#264653] text-sm">Verification Timeline</h4>
                                    <p className="text-gray-500 text-xs mt-0.5">Refunds are processed within 5-7 business days after successful verification.</p>
                                </div>
                            </div>

                            {/* Step 3 */}
                            <div className="flex gap-4">
                                <div className="flex flex-col items-center">
                                    <span className="w-8 h-8 rounded-full bg-[#264653] text-white flex items-center justify-center font-bold text-xs shadow-sm">3</span>
                                </div>
                                <div className="pt-0.5">
                                    <h4 className="font-bold text-[#264653] text-sm">Refund Method & Amount</h4>
                                    <p className="text-gray-500 text-xs mt-0.5">
                                        Refunds are issued via your original payment method. Only the order amount is refunded (delivery charges may not be refunded).
                                    </p>
                                </div>
                            </div>
                        </div>
                    </div>
                </section>
            </div>

            {/* Cancellation Policy Section */}
            <section className="bg-white border border-gray-100 p-8 md:p-10 rounded-3xl shadow-md hover:shadow-lg transition-all duration-300 space-y-8 hover:scale-[1.005]">
                <div className="flex items-center gap-3">
                    <span className="p-3 bg-orange-50 rounded-2xl text-2xl text-[#F4A261]">
                        <FaRegClock />
                    </span>
                    <div>
                        <h2 className="font-heading font-extrabold text-2xl text-[#264653]">
                            Cancellation Policy
                        </h2>
                        <p className="text-gray-500 text-xs mt-0.5">Review timelines for cancelling orders</p>
                    </div>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
                    {/* Stage 1 */}
                    <div className="card bg-gray-50/50 p-6 border border-gray-100 space-y-3 relative overflow-hidden group">
                        <div className="absolute top-0 left-0 w-full h-1 bg-[#F4A261] group-hover:h-1.5 transition-all"></div>
                        <span className="text-2xl block text-[#F4A261]"><FaComments /></span>
                        <h3 className="font-bold text-base text-[#264653]">Before Confirmation Call</h3>
                        <p className="text-gray-500 text-xs leading-relaxed">
                            You can cancel your order anytime before we place the confirmation call to your phone.
                        </p>
                    </div>

                    {/* Stage 2 */}
                    <div className="card bg-gray-50/50 p-6 border border-gray-100 space-y-3 relative overflow-hidden group">
                        <div className="absolute top-0 left-0 w-full h-1 bg-[#E63946] group-hover:h-1.5 transition-all"></div>
                        <span className="text-2xl block text-[#E63946]"><FaPhoneAlt /></span>
                        <h3 className="font-bold text-base text-[#264653]">During Confirmation Call</h3>
                        <p className="text-gray-500 text-xs leading-relaxed">
                            You can cancel immediately by informing our customer representative during the confirmation call.
                        </p>
                    </div>

                    {/* Stage 3 */}
                    <div className="card bg-gray-50/50 p-6 border border-gray-100 space-y-3 relative overflow-hidden group">
                        <div className="absolute top-0 left-0 w-full h-1 bg-[#264653] group-hover:h-1.5 transition-all"></div>
                        <span className="text-2xl block text-[#264653]"><FaHandshake /></span>
                        <h3 className="font-bold text-base text-[#264653]">After Confirmation</h3>
                        <p className="text-gray-500 text-xs leading-relaxed">
                            Cancellation request is subject to the management team's discretion once confirmed.
                        </p>
                    </div>

                    {/* Stage 4 */}
                    <div className="card bg-gray-50/50 p-6 border border-gray-100 space-y-3 relative overflow-hidden group">
                        <div className="absolute top-0 left-0 w-full h-1 bg-gray-400 group-hover:h-1.5 transition-all"></div>
                        <span className="text-2xl block text-gray-400"><FaPizzaSlice /></span>
                        <h3 className="font-bold text-base text-[#264653]">Processing Orders</h3>
                        <p className="text-gray-500 text-xs leading-relaxed">
                            Cancellation may not be possible once the kitchen has started preparing your fresh meals.
                        </p>
                    </div>
                </div>
            </section>

            {/* Contact Section at Bottom */}
            <section className="relative rounded-3xl bg-gradient-to-br from-[#264653] to-slate-900 overflow-hidden shadow-xl text-white p-8 md:p-12 text-center space-y-6">
                <div className="max-w-2xl mx-auto space-y-4">
                    <h3 className="font-heading font-extrabold text-2xl md:text-3xl text-white">
                        Have Questions or Need Assistance?
                    </h3>
                    <p className="text-gray-300 text-sm md:text-base max-w-lg mx-auto">
                        For any questions, contact us at <span className="text-[#F4A261] font-semibold">thehungryhub26@gmail.com</span> or call <span className="text-[#F4A261] font-semibold">0336-0357333</span>. We're here to help!
                    </p>
                </div>
                <div className="flex flex-col sm:flex-row items-center justify-center gap-4 pt-2">
                    <a
                        href="mailto:thehungryhub26@gmail.com"
                        className="w-full sm:w-auto px-6 py-3.5 bg-[#E63946] hover:bg-[#c92f3b] text-white font-bold rounded-xl flex items-center justify-center gap-2 shadow-lg transition-transform hover:scale-[1.03]"
                    >
                        <FaEnvelope className="shrink-0" /> Email Support
                    </a>
                    <a
                        href="tel:03360357333"
                        className="w-full sm:w-auto px-6 py-3.5 bg-[#264653] border border-gray-700 hover:border-gray-500 text-white font-bold rounded-xl flex items-center justify-center gap-2 shadow-lg transition-transform hover:scale-[1.03]"
                    >
                        <FaPhoneAlt className="shrink-0" /> Call 0336-0357333
                    </a>
                </div>
            </section>
        </div>
    );
}

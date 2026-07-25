// E:\hungryHub\hungry-fast-food\website\frontend\src\components\common\ToastNotification.jsx

import React, { useEffect } from 'react';

export default function ToastNotification({ message, type = 'success', onClose, duration = 4000 }) {
    useEffect(() => {
        const timer = setTimeout(() => {
            onClose();
        }, duration);

        return () => clearTimeout(timer);
    }, [duration, onClose]);

    const bgClass = type === 'success' ? 'bg-green-500' : 'bg-primary';
    const icon = type === 'success' ? '✅' : '❌';

    return (
        <div className={`fixed bottom-5 right-5 z-50 flex items-center gap-3 px-5 py-3.5 rounded-xl shadow-2xl text-white font-semibold animate-slide-up bg-opacity-95 backdrop-blur-md transition-all duration-300 border border-white border-opacity-10 max-w-sm md:max-w-md ${bgClass}`}>
            <span className="text-xl">{icon}</span>
            <p className="text-sm flex-grow leading-tight pr-2">{message}</p>
            <button
                onClick={onClose}
                className="text-white hover:text-gray-200 transition-colors focus:outline-none p-1 hover:bg-white hover:bg-opacity-10 rounded"
            >
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={2.5} stroke="currentColor" className="w-4 h-4">
                    <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
                </svg>
            </button>
            {/* Progress bar */}
            <div className="absolute bottom-0 left-0 h-1 bg-white bg-opacity-30 rounded-b-xl animate-width" style={{ animationDuration: `${duration}ms`, animationName: 'shrinkWidth', animationTimingFunction: 'linear' }}></div>
            
            <style dangerouslySetInnerHTML={{__html: `
                @keyframes shrinkWidth {
                    from { width: 100%; }
                    to { width: 0%; }
                }
            `}} />
        </div>
    );
}

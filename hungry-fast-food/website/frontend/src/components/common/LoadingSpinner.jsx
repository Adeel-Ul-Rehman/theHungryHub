// E:\hungryHub\hungry-fast-food\website\frontend\src\components\common\LoadingSpinner.jsx

import React from 'react';

export default function LoadingSpinner() {
    return (
        <div className="flex flex-col items-center justify-center min-h-[300px] gap-4">
            <div className="relative w-16 h-16">
                {/* Outer ring */}
                <div className="absolute inset-0 border-4 border-gray-200 rounded-full"></div>
                {/* Spin ring */}
                <div className="absolute inset-0 border-4 border-t-primary border-r-transparent border-b-transparent border-l-transparent rounded-full animate-spin"></div>
                {/* Floating logo icon */}
                <div className="absolute inset-0 flex items-center justify-center p-3 animate-pulse-slow">
                    <img src="/logo.png" alt="loading" className="w-full h-full object-contain" />
                </div>
            </div>
            <p className="text-text-secondary font-semibold text-sm tracking-wide animate-pulse">
                Cooking something delicious...
            </p>
        </div>
    );
}

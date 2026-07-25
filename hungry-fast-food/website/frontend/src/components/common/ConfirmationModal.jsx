// E:\hungryHub\hungry-fast-food\website\frontend\src\components\common\ConfirmationModal.jsx

import React from 'react';

export default function ConfirmationModal({
    isOpen,
    title,
    message,
    confirmText = 'Confirm',
    cancelText = 'Cancel',
    onConfirm,
    onCancel,
    type = 'danger'
}) {
    if (!isOpen) return null;

    const confirmBtnClass = type === 'danger'
        ? 'bg-primary hover:bg-primary-dark text-white hover:shadow-orange-100/50'
        : 'bg-secondary hover:bg-orange-600 text-white hover:shadow-orange-100/50';

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/15 backdrop-blur-xs animate-fade-in">
            <div className="bg-white w-full max-w-sm rounded-2xl shadow-2xl overflow-hidden border border-gray-100 p-6 md:p-8 animate-slide-up text-center space-y-6">
                
                {/* Warning/Info Icon */}
                <div className="w-16 h-16 bg-orange-50 text-primary text-3xl flex items-center justify-center rounded-full mx-auto animate-pulse">
                    {type === 'danger' ? '⚠️' : 'ℹ️'}
                </div>

                <div className="space-y-2">
                    <h3 className="font-heading font-extrabold text-xl text-text-primary">
                        {title}
                    </h3>
                    <p className="text-text-secondary text-sm leading-relaxed">
                        {message}
                    </p>
                </div>

                {/* Actions */}
                <div className="flex gap-3 pt-2">
                    <button
                        onClick={onCancel}
                        className="flex-1 py-3 px-4 rounded-xl font-bold text-xs border border-gray-200 text-text-primary hover:bg-gray-50 transition-all duration-300"
                    >
                        {cancelText}
                    </button>
                    <button
                        onClick={onConfirm}
                        className={`flex-1 py-3 px-4 rounded-xl font-bold text-xs shadow-md transition-all duration-300 active:scale-95 ${confirmBtnClass}`}
                    >
                        {confirmText}
                    </button>
                </div>
            </div>
        </div>
    );
}

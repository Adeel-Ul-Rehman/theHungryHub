// E:\hungryHub\hungry-fast-food\website\frontend\src\components\layout\CategoryFilter.jsx

import React from 'react';

export default function CategoryFilter({ categories, activeCategory, onSelectCategory }) {
    return (
        <div className="flex items-center gap-3 overflow-x-auto pb-4 scrollbar-thin scrollbar-thumb-gray-200">
            {/* All Category Button */}
            <button
                onClick={() => onSelectCategory(null)}
                className={`px-6 py-2.5 rounded-full font-semibold transition-all duration-300 whitespace-nowrap text-sm ${
                    activeCategory === null
                        ? 'bg-primary text-white shadow-md'
                        : 'bg-white text-text-primary hover:bg-gray-100 border border-gray-200 shadow-sm'
                }`}
            >
                All Menu 🍕
            </button>

            {/* List of Categories */}
            {categories.map((category) => (
                <button
                    key={category.id}
                    onClick={() => onSelectCategory(category.id)}
                    className={`px-6 py-2.5 rounded-full font-semibold transition-all duration-300 whitespace-nowrap text-sm ${
                        activeCategory === category.id
                            ? 'bg-primary text-white shadow-md'
                            : 'bg-white text-text-primary hover:bg-gray-100 border border-gray-200 shadow-sm'
                    }`}
                >
                    {category.name}
                </button>
            ))}
        </div>
    );
}

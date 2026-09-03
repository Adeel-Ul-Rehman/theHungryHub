import React, { useState } from 'react';
import { UtensilsCrossed, Tag, CheckCircle2, XCircle } from 'lucide-react';

const mockCategories = [
  { id: 1, name: 'Burgers', count: 12 },
  { id: 2, name: 'Pizzas', count: 8 },
  { id: 3, name: 'Deals & Combos', count: 6 },
  { id: 4, name: 'Fried Chicken', count: 5 },
  { id: 5, name: 'Fries & Sides', count: 7 },
  { id: 6, name: 'Cold Drinks', count: 9 },
];

const mockProducts = [
  { id: 101, name: 'Zinger Burger', category: 'Burgers', price: 550, isAvailable: true, description: 'Crispy fried chicken thigh fillet with garlic mayo sauce in sesame bun.' },
  { id: 102, name: 'Beef Cheese Burger', category: 'Burgers', price: 680, isAvailable: true, description: '100% pure beef patty topped with melted cheddar cheese slice and pickles.' },
  { id: 103, name: 'Chicken Tikka Pizza', category: 'Pizzas', price: 1250, isAvailable: true, description: 'Spicy chicken tikka chunks with mozzarella cheese, onions and green bell peppers.' },
  { id: 104, name: 'Double Trouble Deal 1', category: 'Deals & Combos', price: 1450, isAvailable: true, description: '2 Zinger Burgers + 1 Large French Fries + 2 Cold Drinks.' },
  { id: 105, name: 'Crispy Chicken Wings (6 pcs)', category: 'Fried Chicken', price: 490, isAvailable: true, description: 'Gold crisp fried chicken wings served with dip.' },
  { id: 106, name: 'Loaded Masala Fries', category: 'Fries & Sides', price: 380, isAvailable: false, description: 'Crispy french fries tossed in special spicy masala mix.' },
];

const MenuPage = () => {
  const [selectedCategory, setSelectedCategory] = useState('Burgers');

  const filteredProducts = mockProducts.filter(p => p.category === selectedCategory || selectedCategory === 'All');

  return (
    <div className="p-8 space-y-8">
      {/* Header */}
      <div className="p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm">
        <h2 className="text-xl font-black text-slate-900 dark:text-white flex items-center gap-2">
          <UtensilsCrossed className="w-6 h-6 text-orange-500" /> Live Restaurant Menu & Price Catalog
        </h2>
        <p className="text-xs text-slate-500 dark:text-slate-400 mt-1">Overview of active categories, products, prices, and stock availability</p>
      </div>

      {/* Category Tabs */}
      <div className="flex items-center gap-3 overflow-x-auto pb-2">
        <button
          onClick={() => setSelectedCategory('All')}
          className={`px-4 py-2 rounded-xl text-xs font-bold whitespace-nowrap transition-all ${
            selectedCategory === 'All'
              ? 'bg-orange-500 text-white shadow-md shadow-orange-500/20'
              : 'bg-white dark:bg-slate-900 text-slate-600 dark:text-slate-400 border border-slate-200 dark:border-slate-800'
          }`}
        >
          All Items ({mockProducts.length})
        </button>
        {mockCategories.map((cat) => (
          <button
            key={cat.id}
            onClick={() => setSelectedCategory(cat.name)}
            className={`px-4 py-2 rounded-xl text-xs font-bold whitespace-nowrap transition-all ${
              selectedCategory === cat.name
                ? 'bg-orange-500 text-white shadow-md shadow-orange-500/20'
                : 'bg-white dark:bg-slate-900 text-slate-600 dark:text-slate-400 border border-slate-200 dark:border-slate-800'
            }`}
          >
            {cat.name} ({cat.count})
          </button>
        ))}
      </div>

      {/* Product Cards Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {filteredProducts.map((prod) => (
          <div key={prod.id} className="p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm flex flex-col justify-between space-y-4">
            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <span className="px-2.5 py-0.5 rounded-full text-[10px] font-black uppercase bg-orange-50 text-orange-600 dark:bg-orange-950/50 dark:text-orange-400">
                  {prod.category}
                </span>
                {prod.isAvailable ? (
                  <span className="flex items-center gap-1 text-[11px] font-extrabold text-emerald-600 dark:text-emerald-400">
                    <CheckCircle2 className="w-3.5 h-3.5" /> Available
                  </span>
                ) : (
                  <span className="flex items-center gap-1 text-[11px] font-extrabold text-rose-500">
                    <XCircle className="w-3.5 h-3.5" /> Sold Out
                  </span>
                )}
              </div>
              <h3 className="font-extrabold text-base text-slate-900 dark:text-white">{prod.name}</h3>
              <p className="text-xs text-slate-500 dark:text-slate-400 leading-relaxed">{prod.description}</p>
            </div>

            <div className="pt-3 border-t border-slate-100 dark:border-slate-800 flex items-center justify-between">
              <span className="text-xs font-bold text-slate-400">Standard Price</span>
              <span className="text-lg font-black text-orange-600 dark:text-orange-400">PKR {prod.price}</span>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default MenuPage;

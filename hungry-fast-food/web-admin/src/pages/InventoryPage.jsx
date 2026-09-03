import React, { useState } from 'react';
import { 
  Boxes, 
  FileText, 
  FileSpreadsheet, 
  FileCode, 
  Search, 
  AlertTriangle, 
  CheckCircle, 
  XCircle,
  Plus
} from 'lucide-react';
import { exportToCSV, exportToExcel, exportToPDF } from '../utils/exportUtils';

const initialMaterials = [
  { id: 1, name: 'Burger Buns', category: 'Bakery', stock: 120, unit: 'units', min: 30, cost: 25 },
  { id: 2, name: 'Chicken Fillets', category: 'Meat & Poultry', stock: 15000, unit: 'grams (g)', min: 3000, cost: 1.2 },
  { id: 3, name: 'Beef Patties', category: 'Meat & Poultry', stock: 10000, unit: 'grams (g)', min: 2000, cost: 1.5 },
  { id: 4, name: 'Cheddar Cheese Slices', category: 'Dairy', stock: 200, unit: 'units', min: 40, cost: 15 },
  { id: 5, name: 'Pizza Mozzarella Cheese', category: 'Dairy', stock: 1800, unit: 'grams (g)', min: 2000, cost: 1.8 },
  { id: 6, name: 'French Fries (Frozen)', category: 'Vegetables', stock: 20000, unit: 'grams (g)', min: 5000, cost: 0.6 },
  { id: 7, name: 'Garlic Mayo Sauce', category: 'Sauces & Spices', stock: 800, unit: 'ml', min: 1000, cost: 0.5 },
  { id: 8, name: 'Cooking Oil', category: 'Pantry', stock: 15000, unit: 'ml', min: 3000, cost: 0.4 },
  { id: 9, name: 'Burger Packaging Boxes', category: 'Packaging', stock: 300, unit: 'units', min: 50, cost: 8 },
];

const InventoryPage = () => {
  const [materials, setMaterials] = useState(initialMaterials);
  const [search, setSearch] = useState('');
  const [selectedCategory, setSelectedCategory] = useState('All');
  const [restockModalItem, setRestockModalItem] = useState(null);
  const [restockQty, setRestockQty] = useState(100);

  const categories = ['All', 'Bakery', 'Meat & Poultry', 'Dairy', 'Sauces & Spices', 'Vegetables', 'Pantry', 'Packaging'];

  const filteredMaterials = materials.filter(item => {
    const matchesSearch = item.name.toLowerCase().includes(search.toLowerCase()) || item.category.toLowerCase().includes(search.toLowerCase());
    const matchesCat = selectedCategory === 'All' || item.category === selectedCategory;
    return matchesSearch && matchesCat;
  });

  const handleExportCSV = () => {
    const exportData = filteredMaterials.map(m => ({
      ID: m.id, Name: m.name, Category: m.category, Stock: m.stock, Unit: m.unit, MinAlert: m.min, UnitCost: m.cost, Valuation: m.stock * m.cost
    }));
    exportToCSV(exportData, 'HungryHub_Inventory_Stock');
  };

  const handleExportExcel = () => {
    const exportData = filteredMaterials.map(m => ({
      ID: m.id, Name: m.name, Category: m.category, Stock: m.stock, Unit: m.unit, MinAlert: m.min, UnitCost: m.cost, Valuation: m.stock * m.cost
    }));
    exportToExcel(exportData, 'HungryHub_Inventory_Stock', 'RawMaterials');
  };

  const handleExportPDF = () => {
    const headers = ['ID', 'Ingredient Name', 'Category', 'Stock Level', 'Unit', 'Alert Level', 'Valuation (PKR)'];
    const rows = filteredMaterials.map(m => [
      m.id, m.name, m.category, m.stock.toLocaleString(), m.unit, m.min.toLocaleString(), `PKR ${(m.stock * m.cost).toLocaleString()}`
    ]);
    exportToPDF('Raw Material Inventory Stock Audit Report', headers, rows, 'HungryHub_Inventory_Stock_Report');
  };

  const handleApplyRestock = () => {
    if (!restockModalItem) return;
    setMaterials(prev => prev.map(m => m.id === restockModalItem.id ? { ...m, stock: m.stock + Number(restockQty) } : m));
    setRestockModalItem(null);
  };

  return (
    <div className="p-4 sm:p-8 space-y-6 sm:space-y-8">
      
      {/* Header & Export Toolbar */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 p-5 sm:p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm">
        <div>
          <h2 className="text-base sm:text-xl font-extrabold text-slate-900 dark:text-white flex items-center gap-2">
            <Boxes className="w-5 h-5 sm:w-6 sm:h-6 text-orange-500 shrink-0" /> Domino's & KFC Raw Material Inventory
          </h2>
          <p className="text-xs text-slate-400 mt-0.5">Live stock control, alert levels, and multi-format report exports.</p>
        </div>

        {/* Compact Export Buttons */}
        <div className="flex items-center gap-2 flex-wrap">
          <button
            onClick={handleExportPDF}
            className="flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-rose-50 dark:bg-rose-950/40 border border-rose-200 dark:border-rose-800/50 text-rose-600 dark:text-rose-400 font-extrabold text-xs hover:bg-rose-100 transition-colors"
          >
            <FileText className="w-3.5 h-3.5" /> PDF
          </button>
          <button
            onClick={handleExportExcel}
            className="flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-emerald-50 dark:bg-emerald-950/40 border border-emerald-200 dark:border-emerald-800/50 text-emerald-600 dark:text-emerald-400 font-extrabold text-xs hover:bg-emerald-100 transition-colors"
          >
            <FileSpreadsheet className="w-3.5 h-3.5" /> Excel
          </button>
          <button
            onClick={handleExportCSV}
            className="flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-blue-50 dark:bg-blue-950/40 border border-blue-200 dark:border-blue-800/50 text-blue-600 dark:text-blue-400 font-extrabold text-xs hover:bg-blue-100 transition-colors"
          >
            <FileCode className="w-3.5 h-3.5" /> CSV
          </button>
        </div>
      </div>

      {/* Filter & Search Bar */}
      <div className="flex flex-col sm:flex-row items-center justify-between gap-3">
        <div className="relative w-full sm:w-80">
          <Search className="w-4 h-4 absolute left-3.5 top-3 text-slate-400" />
          <input
            type="text"
            placeholder="Search raw materials..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full pl-10 pr-4 py-2.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 text-slate-900 dark:text-white text-xs sm:text-sm font-semibold focus:outline-none focus:ring-2 focus:ring-orange-500"
          />
        </div>

        {/* Category Pills Slider */}
        <div className="flex items-center gap-1.5 overflow-x-auto w-full sm:w-auto pb-1 sm:pb-0 scrollbar-none">
          {categories.map((cat) => (
            <button
              key={cat}
              onClick={() => setSelectedCategory(cat)}
              className={`px-3 py-1.5 rounded-xl text-xs font-bold whitespace-nowrap transition-all ${
                selectedCategory === cat
                  ? 'bg-orange-500 text-white shadow-md shadow-orange-500/20'
                  : 'bg-white dark:bg-slate-900 text-slate-600 dark:text-slate-400 border border-slate-200 dark:border-slate-800 hover:bg-slate-50'
              }`}
            >
              {cat}
            </button>
          ))}
        </div>
      </div>

      {/* Raw Materials Inventory Table — Whitespace-nowrap scrollable table */}
      <div className="p-4 sm:p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse whitespace-nowrap">
            <thead>
              <tr className="border-b border-slate-100 dark:border-slate-800 text-[10px] sm:text-xs font-black text-slate-400 uppercase tracking-wider">
                <th className="py-3 px-3">Ingredient</th>
                <th className="py-3 px-3">Category</th>
                <th className="py-3 px-3">Current Stock</th>
                <th className="py-3 px-3">Alert Threshold</th>
                <th className="py-3 px-3">Status</th>
                <th className="py-3 px-3">Valuation</th>
                <th className="py-3 px-3 text-right">Action</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 dark:divide-slate-800/80 text-xs sm:text-sm">
              {filteredMaterials.map((item) => {
                const isLow = item.stock <= item.min;
                const isOut = item.stock <= 0;
                const stockPercent = Math.min(100, Math.round((item.stock / (item.min * 3)) * 100));

                return (
                  <tr key={item.id} className="hover:bg-slate-50 dark:hover:bg-slate-800/50 transition-colors">
                    <td className="py-3.5 px-3 font-extrabold text-slate-900 dark:text-white">{item.name}</td>
                    <td className="py-3.5 px-3 font-medium text-slate-500">{item.category}</td>
                    <td className="py-3.5 px-3 font-black text-slate-900 dark:text-white">
                      {item.stock.toLocaleString()} <span className="text-[10px] font-normal text-slate-400">{item.unit}</span>
                      <div className="w-24 h-1.5 bg-slate-100 dark:bg-slate-800 rounded-full mt-1 overflow-hidden">
                        <div 
                          className={`h-full rounded-full ${isOut ? 'bg-rose-500' : isLow ? 'bg-amber-500' : 'bg-emerald-500'}`}
                          style={{ width: `${Math.max(5, stockPercent)}%` }}
                        />
                      </div>
                    </td>
                    <td className="py-3.5 px-3 font-semibold text-slate-500">{item.min.toLocaleString()} {item.unit}</td>
                    <td className="py-3.5 px-3">
                      {isOut ? (
                        <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-black bg-rose-100 text-rose-700 dark:bg-rose-950/60 dark:text-rose-400 uppercase">
                          <XCircle className="w-3 h-3" /> OUT OF STOCK
                        </span>
                      ) : isLow ? (
                        <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-black bg-amber-100 text-amber-700 dark:bg-amber-950/60 dark:text-amber-400 uppercase">
                          <AlertTriangle className="w-3 h-3" /> LOW STOCK
                        </span>
                      ) : (
                        <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-[10px] font-black bg-emerald-100 text-emerald-700 dark:bg-emerald-950/60 dark:text-emerald-400 uppercase">
                          <CheckCircle className="w-3 h-3" /> IN STOCK
                        </span>
                      )}
                    </td>
                    <td className="py-3.5 px-3 font-black text-slate-900 dark:text-white">
                      PKR {(item.stock * item.cost).toLocaleString()}
                    </td>
                    <td className="py-3.5 px-3 text-right">
                      <button
                        onClick={() => setRestockModalItem(item)}
                        className="px-2.5 py-1 rounded-lg bg-orange-50 dark:bg-orange-950/40 text-orange-600 dark:text-orange-400 font-extrabold text-[11px] hover:bg-orange-100 transition-colors"
                      >
                        + Restock
                      </button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </div>

      {/* Quick Restock Modal */}
      {restockModalItem && (
        <div className="fixed inset-0 z-50 bg-slate-950/60 backdrop-blur-sm flex items-center justify-center p-4">
          <div className="w-full max-w-md p-6 rounded-3xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-2xl space-y-6">
            <h3 className="text-base font-extrabold text-slate-900 dark:text-white">📦 Restock {restockModalItem.name}</h3>
            <div className="space-y-4">
              <div>
                <label className="text-xs font-bold text-slate-500 uppercase">Quantity to Add ({restockModalItem.unit})</label>
                <input
                  type="number"
                  value={restockQty}
                  onChange={(e) => setRestockQty(e.target.value)}
                  className="w-full mt-1.5 px-4 py-2.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50 dark:bg-slate-800 font-bold text-sm"
                />
              </div>
            </div>
            <div className="flex items-center gap-3 pt-2">
              <button
                onClick={() => setRestockModalItem(null)}
                className="w-1/2 py-2.5 rounded-xl border border-slate-200 dark:border-slate-800 text-slate-600 dark:text-slate-400 font-bold text-xs"
              >
                Cancel
              </button>
              <button
                onClick={handleApplyRestock}
                className="w-1/2 py-2.5 rounded-xl bg-orange-500 text-white font-bold text-xs shadow-lg shadow-orange-500/25"
              >
                Confirm Restock
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default InventoryPage;

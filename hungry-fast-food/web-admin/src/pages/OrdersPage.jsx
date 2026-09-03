import React, { useState } from 'react';
import { 
  ShoppingBag, 
  FileText, 
  FileSpreadsheet, 
  FileCode, 
  Search
} from 'lucide-react';
import { exportToCSV, exportToExcel, exportToPDF } from '../utils/exportUtils';

const mockAllOrders = [
  { id: 'ORD-9842', customer: 'Ali Hassan', phone: '0300-1234567', type: 'Delivery', address: 'F-8/3, Islamabad', total: 2450, status: 'delivered', date: '2026-09-03 21:15' },
  { id: 'ORD-9841', customer: 'Usman Malik', phone: '0321-9876543', type: 'Takeaway', address: 'Counter Takeaway', total: 1850, status: 'preparing', date: '2026-09-03 21:22' },
  { id: 'ORD-9840', customer: 'Fatima Zahra', phone: '0333-5551234', type: 'Dining', address: 'Table 4', total: 3200, status: 'delivered', date: '2026-09-03 20:45' },
  { id: 'ORD-9839', customer: 'Hamza Khan', phone: '0312-4447788', type: 'Delivery', address: 'G-11/2, Islamabad', total: 1200, status: 'pending', date: '2026-09-03 21:30' },
  { id: 'ORD-9838', customer: 'Sara Ahmed', phone: '0301-6663322', type: 'Delivery', address: 'I-8/4, Islamabad', total: 2900, status: 'cancelled', date: '2026-09-03 19:10' },
  { id: 'ORD-9837', customer: 'Zayn Riaz', phone: '0345-1112233', type: 'Takeaway', address: 'Counter Takeaway', total: 4500, status: 'delivered', date: '2026-09-03 18:30' },
];

const OrdersPage = () => {
  const [orders] = useState(mockAllOrders);
  const [statusFilter, setStatusFilter] = useState('All');
  const [search, setSearch] = useState('');

  const statuses = ['All', 'delivered', 'preparing', 'pending', 'cancelled'];

  const filteredOrders = orders.filter(ord => {
    const matchesSearch = ord.id.toLowerCase().includes(search.toLowerCase()) || ord.customer.toLowerCase().includes(search.toLowerCase());
    const matchesStatus = statusFilter === 'All' || ord.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const handleExportCSV = () => {
    const exportData = filteredOrders.map(o => ({
      OrderID: o.id, Customer: o.customer, Phone: o.phone, Type: o.type, TotalPKR: o.total, Status: o.status, Date: o.date
    }));
    exportToCSV(exportData, 'HungryHub_Sales_Orders');
  };

  const handleExportExcel = () => {
    const exportData = filteredOrders.map(o => ({
      OrderID: o.id, Customer: o.customer, Phone: o.phone, Type: o.type, TotalPKR: o.total, Status: o.status, Date: o.date
    }));
    exportToExcel(exportData, 'HungryHub_Sales_Orders', 'OrdersHistory');
  };

  const handleExportPDF = () => {
    const headers = ['Order ID', 'Customer Name', 'Type', 'Total (PKR)', 'Status', 'Date & Time'];
    const rows = filteredOrders.map(o => [
      o.id, o.customer, o.type, `PKR ${o.total.toLocaleString()}`, o.status.toUpperCase(), o.date
    ]);
    exportToPDF('Sales & Orders History Audit Report', headers, rows, 'HungryHub_Sales_Orders_Report');
  };

  return (
    <div className="p-4 sm:p-8 space-y-6 sm:space-y-8">
      
      {/* Header & Export Toolbar */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 p-5 sm:p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm">
        <div>
          <h2 className="text-base sm:text-xl font-extrabold text-slate-900 dark:text-white flex items-center gap-2">
            <ShoppingBag className="w-5 h-5 sm:w-6 sm:h-6 text-orange-500 shrink-0" /> Sales & Order Audit Logs
          </h2>
          <p className="text-xs text-slate-400 mt-0.5">Track kitchen order history, delivered revenue, and export sales reports.</p>
        </div>

        {/* Export Buttons */}
        <div className="flex items-center gap-2 flex-wrap">
          <button
            onClick={handleExportPDF}
            className="flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-rose-50 dark:bg-rose-950/40 border border-rose-200 dark:border-rose-800/50 text-rose-600 dark:text-rose-400 font-extrabold text-xs hover:bg-rose-100 transition-colors"
          >
            <FileText className="w-3.5 h-3.5" /> Sales PDF
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
            placeholder="Search Order ID or Customer..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full pl-10 pr-4 py-2.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-white dark:bg-slate-900 text-slate-900 dark:text-white text-xs sm:text-sm font-semibold focus:outline-none focus:ring-2 focus:ring-orange-500"
          />
        </div>

        {/* Status Filter Tabs */}
        <div className="flex items-center gap-1.5 overflow-x-auto w-full sm:w-auto pb-1 sm:pb-0 scrollbar-none">
          {statuses.map((st) => (
            <button
              key={st}
              onClick={() => setStatusFilter(st)}
              className={`px-3 py-1.5 rounded-xl text-xs font-bold capitalize whitespace-nowrap transition-all ${
                statusFilter === st
                  ? 'bg-orange-500 text-white shadow-md shadow-orange-500/20'
                  : 'bg-white dark:bg-slate-900 text-slate-600 dark:text-slate-400 border border-slate-200 dark:border-slate-800 hover:bg-slate-50'
              }`}
            >
              {st}
            </button>
          ))}
        </div>
      </div>

      {/* Orders Table — Whitespace-nowrap scrollable table */}
      <div className="p-4 sm:p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm overflow-hidden">
        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse whitespace-nowrap">
            <thead>
              <tr className="border-b border-slate-100 dark:border-slate-800 text-[10px] sm:text-xs font-black text-slate-400 uppercase tracking-wider">
                <th className="py-3 px-3">Order ID</th>
                <th className="py-3 px-3">Customer</th>
                <th className="py-3 px-3">Type & Location</th>
                <th className="py-3 px-3">Total</th>
                <th className="py-3 px-3">Status</th>
                <th className="py-3 px-3 text-right">Date & Time</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 dark:divide-slate-800/80 text-xs sm:text-sm">
              {filteredOrders.map((ord) => (
                <tr key={ord.id} className="hover:bg-slate-50 dark:hover:bg-slate-800/50 transition-colors">
                  <td className="py-3.5 px-3 font-extrabold text-slate-900 dark:text-white">{ord.id}</td>
                  <td className="py-3.5 px-3 font-bold text-slate-900 dark:text-white">{ord.customer}</td>
                  <td className="py-3.5 px-3 font-medium text-slate-500">{ord.type} — {ord.address}</td>
                  <td className="py-3.5 px-3 font-black text-slate-900 dark:text-white">PKR {ord.total.toLocaleString()}</td>
                  <td className="py-3.5 px-3">
                    <span className={`px-2.5 py-0.5 rounded-full text-[10px] font-black uppercase ${
                      ord.status === 'delivered' ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-950/50 dark:text-emerald-400' :
                      ord.status === 'preparing' ? 'bg-amber-100 text-amber-700 dark:bg-amber-950/50 dark:text-amber-400' :
                      ord.status === 'pending' ? 'bg-blue-100 text-blue-700 dark:bg-blue-950/50 dark:text-blue-400' :
                      'bg-rose-100 text-rose-700 dark:bg-rose-950/50 dark:text-rose-400'
                    }`}>
                      {ord.status}
                    </span>
                  </td>
                  <td className="py-3.5 px-3 text-[11px] font-semibold text-slate-400 text-right">{ord.date}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default OrdersPage;

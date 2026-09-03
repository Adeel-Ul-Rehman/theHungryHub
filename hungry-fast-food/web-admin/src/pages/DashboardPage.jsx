import React, { useState, useEffect } from 'react';
import { 
  TrendingUp, 
  ShoppingBag, 
  AlertTriangle, 
  CheckCircle2, 
  XCircle, 
  Clock, 
  ArrowUpRight,
  Flame,
  Zap
} from 'lucide-react';
import { 
  AreaChart, 
  Area, 
  XAxis, 
  YAxis, 
  CartesianGrid, 
  Tooltip, 
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  Legend
} from 'recharts';

const mockSalesData = [
  { day: 'Mon', revenue: 42000, orders: 32 },
  { day: 'Tue', revenue: 58000, orders: 45 },
  { day: 'Wed', revenue: 51000, orders: 38 },
  { day: 'Thu', revenue: 64000, orders: 52 },
  { day: 'Fri', revenue: 89000, orders: 74 },
  { day: 'Sat', revenue: 112000, orders: 98 },
  { day: 'Sun', revenue: 95000, orders: 81 },
];

const mockOrderStatusData = [
  { name: 'Delivered', value: 340, color: '#10b981' },
  { name: 'Preparing', value: 12, color: '#f59e0b' },
  { name: 'Pending', value: 8, color: '#3b82f6' },
  { name: 'Canceled', value: 15, color: '#ef4444' },
];

const mockRecentOrders = [
  { id: 'ORD-9842', customer: 'Ali Hassan', type: 'Delivery', total: 2450, status: 'delivered', time: '10 mins ago' },
  { id: 'ORD-9841', customer: 'Usman Malik', type: 'Takeaway', total: 1850, status: 'preparing', time: '18 mins ago' },
  { id: 'ORD-9840', customer: 'Fatima Zahra', type: 'Dining', total: 3200, status: 'delivered', time: '35 mins ago' },
  { id: 'ORD-9839', customer: 'Hamza Khan', type: 'Delivery', total: 1200, status: 'pending', time: '42 mins ago' },
  { id: 'ORD-9838', customer: 'Sara Ahmed', type: 'Delivery', total: 2900, status: 'cancelled', time: '1 hr ago' },
];

const DashboardPage = () => {
  const [stats, setStats] = useState({
    totalRevenue: 511000,
    totalOrders: 420,
    deliveredOrders: 385,
    canceledOrders: 15,
    lowStockCount: 2,
  });

  return (
    <div className="p-8 space-y-8">
      {/* Top Banner */}
      <div className="relative overflow-hidden rounded-3xl bg-gradient-to-r from-orange-600 via-amber-600 to-amber-500 p-8 text-white shadow-xl shadow-orange-500/15">
        <div className="relative z-10 max-w-2xl space-y-3">
          <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-white/20 backdrop-blur-md text-xs font-bold uppercase tracking-wider text-amber-100">
            <SparklesIcon className="w-3.5 h-3.5" /> Operations Hub Live
          </span>
          <h1 className="text-3xl font-extrabold tracking-tight">Hungry Hub Executive Dashboard</h1>
          <p className="text-sm text-orange-100 font-medium leading-relaxed">
            Monitor real-time sales revenue, track kitchen order queues, inventory alerts, and AI restock recommendations seamlessly.
          </p>
        </div>
        <Flame className="absolute -right-8 -bottom-10 w-64 h-64 text-white/10 pointer-events-none" />
      </div>

      {/* KPI Cards Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {/* Total Revenue */}
        <div className="p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm space-y-4">
          <div className="flex items-center justify-between">
            <span className="text-xs font-bold text-slate-500 dark:text-slate-400 uppercase tracking-wider">Weekly Revenue</span>
            <div className="w-10 h-10 rounded-xl bg-orange-50 dark:bg-orange-950/50 text-orange-600 dark:text-orange-400 flex items-center justify-center">
              <TrendingUp className="w-5 h-5" />
            </div>
          </div>
          <div>
            <h3 className="text-2xl font-black text-slate-900 dark:text-white">PKR {stats.totalRevenue.toLocaleString()}</h3>
            <p className="text-xs font-semibold text-emerald-600 dark:text-emerald-400 flex items-center gap-1 mt-1">
              <ArrowUpRight className="w-3.5 h-3.5" /> +14.2% from last week
            </p>
          </div>
        </div>

        {/* Total Orders */}
        <div className="p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm space-y-4">
          <div className="flex items-center justify-between">
            <span className="text-xs font-bold text-slate-500 dark:text-slate-400 uppercase tracking-wider">Total Orders</span>
            <div className="w-10 h-10 rounded-xl bg-blue-50 dark:bg-blue-950/50 text-blue-600 dark:text-blue-400 flex items-center justify-center">
              <ShoppingBag className="w-5 h-5" />
            </div>
          </div>
          <div>
            <h3 className="text-2xl font-black text-slate-900 dark:text-white">{stats.totalOrders}</h3>
            <p className="text-xs font-semibold text-slate-500 dark:text-slate-400 mt-1">
              {stats.deliveredOrders} Delivered / {stats.canceledOrders} Canceled
            </p>
          </div>
        </div>

        {/* Delivery Success Rate */}
        <div className="p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm space-y-4">
          <div className="flex items-center justify-between">
            <span className="text-xs font-bold text-slate-500 dark:text-slate-400 uppercase tracking-wider">Fulfillment Rate</span>
            <div className="w-10 h-10 rounded-xl bg-emerald-50 dark:bg-emerald-950/50 text-emerald-600 dark:text-emerald-400 flex items-center justify-center">
              <CheckCircle2 className="w-5 h-5" />
            </div>
          </div>
          <div>
            <h3 className="text-2xl font-black text-slate-900 dark:text-white">96.4%</h3>
            <p className="text-xs font-semibold text-emerald-600 dark:text-emerald-400 mt-1">High Customer Satisfaction</p>
          </div>
        </div>

        {/* Low Stock Warnings */}
        <div className="p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm space-y-4">
          <div className="flex items-center justify-between">
            <span className="text-xs font-bold text-slate-500 dark:text-slate-400 uppercase tracking-wider">Low Stock Alerts</span>
            <div className="w-10 h-10 rounded-xl bg-amber-50 dark:bg-amber-950/50 text-amber-600 dark:text-amber-400 flex items-center justify-center">
              <AlertTriangle className="w-5 h-5" />
            </div>
          </div>
          <div>
            <h3 className="text-2xl font-black text-amber-600 dark:text-amber-400">{stats.lowStockCount} Items</h3>
            <p className="text-xs font-semibold text-amber-600 dark:text-amber-400 mt-1">Needs restock attention</p>
          </div>
        </div>
      </div>

      {/* Analytics Charts Section */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Revenue Trend Area Chart */}
        <div className="lg:col-span-2 p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm space-y-6">
          <div className="flex items-center justify-between">
            <div>
              <h3 className="font-bold text-lg text-slate-900 dark:text-white">Weekly Sales Revenue Trend</h3>
              <p className="text-xs text-slate-500 dark:text-slate-400">Daily gross revenue in PKR</p>
            </div>
            <span className="px-3 py-1 text-xs font-extrabold rounded-lg bg-orange-50 dark:bg-orange-950/50 text-orange-600 dark:text-orange-400">
              7 Days Overview
            </span>
          </div>

          <div className="h-72 w-full">
            <ResponsiveContainer width="100%" height="100%">
              <AreaChart data={mockSalesData} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
                <defs>
                  <linearGradient id="revenueGrad" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#f97316" stopOpacity={0.4}/>
                    <stop offset="95%" stopColor="#f97316" stopOpacity={0}/>
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" stroke="#334155" opacity={0.2} />
                <XAxis dataKey="day" stroke="#94a3b8" fontSize={12} tickLine={false} />
                <YAxis stroke="#94a3b8" fontSize={12} tickLine={false} />
                <Tooltip 
                  contentStyle={{ backgroundColor: '#0f172a', borderColor: '#334155', borderRadius: '12px', color: '#fff' }}
                  formatter={(value) => [`PKR ${value.toLocaleString()}`, 'Revenue']}
                />
                <Area type="monotone" dataKey="revenue" stroke="#f97316" strokeWidth={3} fillOpacity={1} fill="url(#revenueGrad)" />
              </AreaChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* Order Status Distribution Pie Chart */}
        <div className="p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm space-y-6">
          <div>
            <h3 className="font-bold text-lg text-slate-900 dark:text-white">Order Status Distribution</h3>
            <p className="text-xs text-slate-500 dark:text-slate-400">Delivered vs Canceled ratios</p>
          </div>

          <div className="h-64 w-full flex items-center justify-center">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie data={mockOrderStatusData} cx="50%" cy="50%" innerRadius={60} outerRadius={85} paddingAngle={4} dataKey="value">
                  {mockOrderStatusData.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={entry.color} />
                  ))}
                </Pie>
                <Tooltip contentStyle={{ backgroundColor: '#0f172a', borderRadius: '8px', color: '#fff' }} />
                <Legend />
              </PieChart>
            </ResponsiveContainer>
          </div>
        </div>
      </div>

      {/* Recent Orders Feed */}
      <div className="p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm space-y-4">
        <div className="flex items-center justify-between">
          <h3 className="font-bold text-lg text-slate-900 dark:text-white">Recent Kitchen & Online Orders</h3>
          <span className="text-xs font-semibold text-orange-600 dark:text-orange-400 hover:underline cursor-pointer">View All Orders →</span>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="border-b border-slate-100 dark:border-slate-800 text-xs font-extrabold text-slate-400 uppercase tracking-wider">
                <th className="py-3 px-4">Order ID</th>
                <th className="py-3 px-4">Customer</th>
                <th className="py-3 px-4">Type</th>
                <th className="py-3 px-4">Total</th>
                <th className="py-3 px-4">Status</th>
                <th className="py-3 px-4">Time</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 dark:divide-slate-800 text-sm">
              {mockRecentOrders.map((order) => (
                <tr key={order.id} className="hover:bg-slate-50 dark:hover:bg-slate-800/50 transition-colors">
                  <td className="py-3 px-4 font-extrabold text-slate-900 dark:text-white">{order.id}</td>
                  <td className="py-3 px-4 font-semibold text-slate-700 dark:text-slate-300">{order.customer}</td>
                  <td className="py-3 px-4 font-medium text-slate-600 dark:text-slate-400">{order.type}</td>
                  <td className="py-3 px-4 font-bold text-slate-900 dark:text-white">PKR {order.total}</td>
                  <td className="py-3 px-4">
                    <span className={`px-2.5 py-1 rounded-full text-xs font-extrabold uppercase ${
                      order.status === 'delivered' ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-950/50 dark:text-emerald-400' :
                      order.status === 'preparing' ? 'bg-amber-100 text-amber-700 dark:bg-amber-950/50 dark:text-amber-400' :
                      order.status === 'pending' ? 'bg-blue-100 text-blue-700 dark:bg-blue-950/50 dark:text-blue-400' :
                      'bg-rose-100 text-rose-700 dark:bg-rose-950/50 dark:text-rose-400'
                    }`}>
                      {order.status}
                    </span>
                  </td>
                  <td className="py-3 px-4 text-xs font-medium text-slate-500">{order.time}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

const SparklesIcon = ({ className }) => (
  <svg className={className} fill="none" viewBox="0 0 24 24" stroke="currentColor">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z" />
  </svg>
);

export default DashboardPage;

import React, { useState } from 'react';
import { 
  TrendingUp, 
  ShoppingBag, 
  AlertTriangle, 
  CheckCircle2, 
  Flame,
  ArrowUpRight,
  Sparkles
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
  { day: 'Mon', revenue: 42000 },
  { day: 'Tue', revenue: 58000 },
  { day: 'Wed', revenue: 51000 },
  { day: 'Thu', revenue: 64000 },
  { day: 'Fri', revenue: 89000 },
  { day: 'Sat', revenue: 112000 },
  { day: 'Sun', revenue: 95000 },
];

const mockOrderStatusData = [
  { name: 'Delivered', value: 340, color: '#10b981' },
  { name: 'Preparing', value: 12, color: '#f59e0b' },
  { name: 'Pending', value: 8, color: '#3b82f6' },
  { name: 'Canceled', value: 15, color: '#ef4444' },
];

const mockRecentOrders = [
  { id: 'ORD-9842', customer: 'Ali Hassan', type: 'Delivery', total: 2450, status: 'delivered', time: '10m ago' },
  { id: 'ORD-9841', customer: 'Usman Malik', type: 'Takeaway', total: 1850, status: 'preparing', time: '18m ago' },
  { id: 'ORD-9840', customer: 'Fatima Zahra', type: 'Dining', total: 3200, status: 'delivered', time: '35m ago' },
  { id: 'ORD-9839', customer: 'Hamza Khan', type: 'Delivery', total: 1200, status: 'pending', time: '42m ago' },
  { id: 'ORD-9838', customer: 'Sara Ahmed', type: 'Delivery', total: 2900, status: 'cancelled', time: '1h ago' },
];

const DashboardPage = () => {
  const [stats] = useState({
    totalRevenue: 511000,
    totalOrders: 420,
    deliveredOrders: 385,
    canceledOrders: 15,
    lowStockCount: 2,
  });

  return (
    <div className="p-4 sm:p-8 space-y-6 sm:space-y-8">
      
      {/* Sleek Banner */}
      <div className="relative overflow-hidden rounded-2xl sm:rounded-3xl bg-gradient-to-r from-orange-600 via-amber-600 to-amber-500 p-5 sm:p-8 text-white shadow-xl shadow-orange-500/15">
        <div className="relative z-10 space-y-2">
          <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full bg-white/20 backdrop-blur-md text-[10px] font-black uppercase tracking-wider text-amber-100">
            <Sparkles className="w-3 h-3" /> Live Control Center
          </span>
          <h1 className="text-xl sm:text-3xl font-black tracking-tight">Hungry Hub Executive Dashboard</h1>
          <p className="text-xs sm:text-sm text-orange-100 font-medium">Real-time revenue analytics, active kitchen tickets, and stock alerts.</p>
        </div>
        <Flame className="absolute -right-6 -bottom-8 w-44 sm:w-64 h-44 sm:h-64 text-white/10 pointer-events-none" />
      </div>

      {/* KPI Cards Grid — 2 columns on mobile, 4 columns on desktop */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-3 sm:gap-6">
        
        {/* Weekly Revenue */}
        <div className="p-4 sm:p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm space-y-2">
          <div className="flex items-center justify-between">
            <span className="text-[10px] sm:text-xs font-bold text-slate-400 uppercase tracking-wider">Revenue</span>
            <div className="w-8 h-8 sm:w-10 sm:h-10 rounded-xl bg-orange-50 dark:bg-orange-950/50 text-orange-600 dark:text-orange-400 flex items-center justify-center shrink-0">
              <TrendingUp className="w-4 h-4 sm:w-5 sm:h-5" />
            </div>
          </div>
          <div>
            <h3 className="text-base sm:text-2xl font-black text-slate-900 dark:text-white">PKR {stats.totalRevenue.toLocaleString()}</h3>
            <p className="text-[10px] sm:text-xs font-semibold text-emerald-600 dark:text-emerald-400 flex items-center gap-0.5 mt-0.5">
              <ArrowUpRight className="w-3 h-3" /> +14.2%
            </p>
          </div>
        </div>

        {/* Total Orders */}
        <div className="p-4 sm:p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm space-y-2">
          <div className="flex items-center justify-between">
            <span className="text-[10px] sm:text-xs font-bold text-slate-400 uppercase tracking-wider">Orders</span>
            <div className="w-8 h-8 sm:w-10 sm:h-10 rounded-xl bg-blue-50 dark:bg-blue-950/50 text-blue-600 dark:text-blue-400 flex items-center justify-center shrink-0">
              <ShoppingBag className="w-4 h-4 sm:w-5 sm:h-5" />
            </div>
          </div>
          <div>
            <h3 className="text-base sm:text-2xl font-black text-slate-900 dark:text-white">{stats.totalOrders}</h3>
            <p className="text-[10px] sm:text-xs font-semibold text-slate-400 mt-0.5">{stats.deliveredOrders} Delivered</p>
          </div>
        </div>

        {/* Success Rate */}
        <div className="p-4 sm:p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm space-y-2">
          <div className="flex items-center justify-between">
            <span className="text-[10px] sm:text-xs font-bold text-slate-400 uppercase tracking-wider">Success Rate</span>
            <div className="w-8 h-8 sm:w-10 sm:h-10 rounded-xl bg-emerald-50 dark:bg-emerald-950/50 text-emerald-600 dark:text-emerald-400 flex items-center justify-center shrink-0">
              <CheckCircle2 className="w-4 h-4 sm:w-5 sm:h-5" />
            </div>
          </div>
          <div>
            <h3 className="text-base sm:text-2xl font-black text-slate-900 dark:text-white">96.4%</h3>
            <p className="text-[10px] sm:text-xs font-semibold text-emerald-600 dark:text-emerald-400 mt-0.5">High Speed</p>
          </div>
        </div>

        {/* Low Stock Alerts */}
        <div className="p-4 sm:p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm space-y-2">
          <div className="flex items-center justify-between">
            <span className="text-[10px] sm:text-xs font-bold text-slate-400 uppercase tracking-wider">Stock Alerts</span>
            <div className="w-8 h-8 sm:w-10 sm:h-10 rounded-xl bg-amber-50 dark:bg-amber-950/50 text-amber-600 dark:text-amber-400 flex items-center justify-center shrink-0">
              <AlertTriangle className="w-4 h-4 sm:w-5 sm:h-5" />
            </div>
          </div>
          <div>
            <h3 className="text-base sm:text-2xl font-black text-amber-600 dark:text-amber-400">{stats.lowStockCount} Items</h3>
            <p className="text-[10px] sm:text-xs font-semibold text-amber-600 dark:text-amber-400 mt-0.5">Action Needed</p>
          </div>
        </div>
      </div>

      {/* Charts Section */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        
        {/* Revenue Trend Area Chart */}
        <div className="lg:col-span-2 p-4 sm:p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm space-y-4">
          <div className="flex items-center justify-between">
            <div>
              <h3 className="font-extrabold text-sm sm:text-lg text-slate-900 dark:text-white">Weekly Revenue Trend</h3>
              <p className="text-[10px] sm:text-xs text-slate-400">Gross revenue in PKR</p>
            </div>
            <span className="px-2.5 py-1 text-[10px] font-black rounded-lg bg-orange-50 dark:bg-orange-950/50 text-orange-600 dark:text-orange-400 uppercase">
              7 Days
            </span>
          </div>

          <div className="h-56 sm:h-72 w-full">
            <ResponsiveContainer width="100%" height="100%">
              <AreaChart data={mockSalesData} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
                <defs>
                  <linearGradient id="revenueGrad" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#f97316" stopOpacity={0.4}/>
                    <stop offset="95%" stopColor="#f97316" stopOpacity={0}/>
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" stroke="#334155" opacity={0.2} />
                <XAxis dataKey="day" stroke="#94a3b8" fontSize={11} tickLine={false} />
                <YAxis stroke="#94a3b8" fontSize={11} tickLine={false} />
                <Tooltip 
                  contentStyle={{ backgroundColor: '#0f172a', borderColor: '#334155', borderRadius: '12px', color: '#fff', fontSize: '12px' }}
                  formatter={(value) => [`PKR ${value.toLocaleString()}`, 'Revenue']}
                />
                <Area type="monotone" dataKey="revenue" stroke="#f97316" strokeWidth={3} fillOpacity={1} fill="url(#revenueGrad)" />
              </AreaChart>
            </ResponsiveContainer>
          </div>
        </div>

        {/* Order Status Distribution Pie Chart */}
        <div className="p-4 sm:p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm space-y-4">
          <div>
            <h3 className="font-extrabold text-sm sm:text-lg text-slate-900 dark:text-white">Order Status Ratio</h3>
            <p className="text-[10px] sm:text-xs text-slate-400">Delivered vs Canceled</p>
          </div>

          <div className="h-56 sm:h-64 w-full flex items-center justify-center">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie data={mockOrderStatusData} cx="50%" cy="50%" innerRadius={50} outerRadius={75} paddingAngle={4} dataKey="value">
                  {mockOrderStatusData.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={entry.color} />
                  ))}
                </Pie>
                <Tooltip contentStyle={{ backgroundColor: '#0f172a', borderRadius: '8px', color: '#fff', fontSize: '12px' }} />
                <Legend wrapperStyle={{ fontSize: '12px' }} />
              </PieChart>
            </ResponsiveContainer>
          </div>
        </div>
      </div>

      {/* Recent Orders Feed — Whitespace-nowrap single-line scrollable table */}
      <div className="p-4 sm:p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm space-y-4">
        <div className="flex items-center justify-between">
          <h3 className="font-extrabold text-sm sm:text-lg text-slate-900 dark:text-white">Recent Orders Feed</h3>
          <span className="text-xs font-bold text-orange-500 hover:underline cursor-pointer">View All →</span>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse whitespace-nowrap">
            <thead>
              <tr className="border-b border-slate-100 dark:border-slate-800 text-[10px] sm:text-xs font-black text-slate-400 uppercase tracking-wider">
                <th className="py-3 px-3">Order ID</th>
                <th className="py-3 px-3">Customer</th>
                <th className="py-3 px-3">Type</th>
                <th className="py-3 px-3">Total</th>
                <th className="py-3 px-3">Status</th>
                <th className="py-3 px-3">Time</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 dark:divide-slate-800/80 text-xs sm:text-sm">
              {mockRecentOrders.map((order) => (
                <tr key={order.id} className="hover:bg-slate-50 dark:hover:bg-slate-800/50 transition-colors">
                  <td className="py-3 px-3 font-extrabold text-slate-900 dark:text-white">{order.id}</td>
                  <td className="py-3 px-3 font-semibold text-slate-700 dark:text-slate-300">{order.customer}</td>
                  <td className="py-3 px-3 font-medium text-slate-500">{order.type}</td>
                  <td className="py-3 px-3 font-black text-slate-900 dark:text-white">PKR {order.total}</td>
                  <td className="py-3 px-3">
                    <span className={`px-2.5 py-0.5 rounded-full text-[10px] font-black uppercase ${
                      order.status === 'delivered' ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-950/50 dark:text-emerald-400' :
                      order.status === 'preparing' ? 'bg-amber-100 text-amber-700 dark:bg-amber-950/50 dark:text-amber-400' :
                      order.status === 'pending' ? 'bg-blue-100 text-blue-700 dark:bg-blue-950/50 dark:text-blue-400' :
                      'bg-rose-100 text-rose-700 dark:bg-rose-950/50 dark:text-rose-400'
                    }`}>
                      {order.status}
                    </span>
                  </td>
                  <td className="py-3 px-3 text-[11px] font-medium text-slate-400">{order.time}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default DashboardPage;

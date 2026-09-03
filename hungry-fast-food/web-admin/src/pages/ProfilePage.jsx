import React, { useState } from 'react';
import { UserCheck, Shield, KeyRound, Save, CheckCircle2 } from 'lucide-react';
import { useAuth } from '../context/AuthContext';

const ProfilePage = () => {
  const { admin, updateProfile } = useAuth();
  
  const [fullName, setFullName] = useState(admin.fullName || '');
  const [email, setEmail] = useState(admin.email || '');
  const [phone, setPhone] = useState(admin.phone || '');
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [successMsg, setSuccessMsg] = useState('');

  const handleSaveProfile = (e) => {
    e.preventDefault();
    updateProfile({ fullName, email, phone });
    setSuccessMsg('Profile information updated successfully!');
    setTimeout(() => setSuccessMsg(''), 3000);
  };

  const handleChangePassword = (e) => {
    e.preventDefault();
    if (!newPassword) return;
    setSuccessMsg('Security password changed successfully!');
    setCurrentPassword('');
    setNewPassword('');
    setTimeout(() => setSuccessMsg(''), 3000);
  };

  return (
    <div className="p-8 max-w-4xl space-y-8">
      {/* Header */}
      <div className="p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm">
        <h2 className="text-xl font-black text-slate-900 dark:text-white flex items-center gap-2">
          <UserCheck className="w-6 h-6 text-orange-500" /> Admin Profile & Credentials Settings
        </h2>
        <p className="text-xs text-slate-500 dark:text-slate-400 mt-1">Manage administrator access, contact details, and security credentials</p>
      </div>

      {successMsg && (
        <div className="p-4 rounded-xl bg-emerald-50 dark:bg-emerald-950/50 border border-emerald-200 dark:border-emerald-800 text-emerald-700 dark:text-emerald-400 text-sm font-bold flex items-center gap-2">
          <CheckCircle2 className="w-5 h-5" /> {successMsg}
        </div>
      )}

      {/* Profile Form */}
      <div className="p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm space-y-6">
        <h3 className="font-extrabold text-base text-slate-900 dark:text-white flex items-center gap-2">
          <Shield className="w-5 h-5 text-orange-500" /> General Profile Information
        </h3>

        <form onSubmit={handleSaveProfile} className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="text-xs font-bold text-slate-500 uppercase">Administrator Full Name</label>
              <input
                type="text"
                value={fullName}
                onChange={(e) => setFullName(e.target.value)}
                className="w-full mt-1.5 px-4 py-2.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50 dark:bg-slate-800 text-sm font-bold text-slate-900 dark:text-white"
              />
            </div>
            <div>
              <label className="text-xs font-bold text-slate-500 uppercase">Login Email Address</label>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="w-full mt-1.5 px-4 py-2.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50 dark:bg-slate-800 text-sm font-bold text-slate-900 dark:text-white"
              />
            </div>
          </div>

          <div>
            <label className="text-xs font-bold text-slate-500 uppercase">Contact Phone Number</label>
            <input
              type="text"
              value={phone}
              onChange={(e) => setPhone(e.target.value)}
              className="w-full mt-1.5 px-4 py-2.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50 dark:bg-slate-800 text-sm font-bold text-slate-900 dark:text-white"
            />
          </div>

          <button
            type="submit"
            className="flex items-center gap-2 px-5 py-2.5 rounded-xl bg-orange-500 text-white font-bold text-xs shadow-lg shadow-orange-500/25 hover:bg-orange-600 transition-colors"
          >
            <Save className="w-4 h-4" /> Save Profile Details
          </button>
        </form>
      </div>

      {/* Password Change Form */}
      <div className="p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm space-y-6">
        <h3 className="font-extrabold text-base text-slate-900 dark:text-white flex items-center gap-2">
          <KeyRound className="w-5 h-5 text-orange-500" /> Security & Access Password
        </h3>

        <form onSubmit={handleChangePassword} className="space-y-4 max-w-md">
          <div>
            <label className="text-xs font-bold text-slate-500 uppercase">Current Password</label>
            <input
              type="password"
              placeholder="••••••••"
              value={currentPassword}
              onChange={(e) => setCurrentPassword(e.target.value)}
              className="w-full mt-1.5 px-4 py-2.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50 dark:bg-slate-800 text-sm font-bold text-slate-900 dark:text-white"
            />
          </div>
          <div>
            <label className="text-xs font-bold text-slate-500 uppercase">New Password</label>
            <input
              type="password"
              placeholder="Enter new password..."
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              className="w-full mt-1.5 px-4 py-2.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50 dark:bg-slate-800 text-sm font-bold text-slate-900 dark:text-white"
            />
          </div>

          <button
            type="submit"
            className="flex items-center gap-2 px-5 py-2.5 rounded-xl bg-slate-900 dark:bg-slate-800 text-white font-bold text-xs hover:bg-slate-800 transition-colors"
          >
            <KeyRound className="w-4 h-4" /> Update Password
          </button>
        </form>
      </div>
    </div>
  );
};

export default ProfilePage;

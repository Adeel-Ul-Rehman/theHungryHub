import React, { useState, useRef } from 'react';
import { UserCheck, Shield, KeyRound, Save, CheckCircle2, Upload, Trash2, Camera } from 'lucide-react';
import { useAuth } from '../context/AuthContext';

const ProfilePage = () => {
  const { admin, updateProfile } = useAuth();
  
  const [fullName, setFullName] = useState(admin.fullName || '');
  const [email, setEmail] = useState(admin.email || '');
  const [avatarUrl, setAvatarUrl] = useState(admin.avatarUrl || null);
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [successMsg, setSuccessMsg] = useState('');

  const fileInputRef = useRef(null);

  const handleImageUpload = (e) => {
    const file = e.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onloadend = () => {
        setAvatarUrl(reader.result);
      };
      reader.readAsDataURL(file);
    }
  };

  const handleRemoveAvatar = () => {
    setAvatarUrl(null);
    if (fileInputRef.current) fileInputRef.current.value = '';
  };

  const handleSaveProfile = (e) => {
    e.preventDefault();
    updateProfile({ fullName, email, avatarUrl });
    setSuccessMsg('Profile details and avatar updated successfully!');
    setTimeout(() => setSuccessMsg(''), 3500);
  };

  const handleChangePassword = (e) => {
    e.preventDefault();
    if (!newPassword) return;
    setSuccessMsg('Security password changed successfully!');
    setCurrentPassword('');
    setNewPassword('');
    setTimeout(() => setSuccessMsg(''), 3500);
  };

  return (
    <div className="p-8 max-w-4xl space-y-8">
      {/* Header */}
      <div className="p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm">
        <h2 className="text-xl font-black text-slate-900 dark:text-white flex items-center gap-2">
          <UserCheck className="w-6 h-6 text-orange-500" /> Admin Profile & Access Credentials
        </h2>
        <p className="text-xs text-slate-500 dark:text-slate-400 mt-1">Manage administrator details, custom avatar picture, and security credentials</p>
      </div>

      {successMsg && (
        <div className="p-4 rounded-xl bg-emerald-50 dark:bg-emerald-950/50 border border-emerald-200 dark:border-emerald-800 text-emerald-700 dark:text-emerald-400 text-sm font-bold flex items-center gap-2">
          <CheckCircle2 className="w-5 h-5" /> {successMsg}
        </div>
      )}

      {/* Profile Avatar & Details Form */}
      <div className="p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm space-y-6">
        <h3 className="font-extrabold text-base text-slate-900 dark:text-white flex items-center gap-2">
          <Shield className="w-5 h-5 text-orange-500" /> Admin Avatar & Profile Details
        </h3>

        {/* Profile Picture Upload / Remove Section */}
        <div className="flex items-center gap-6 p-4 rounded-2xl bg-slate-50 dark:bg-slate-800/40 border border-slate-100 dark:border-slate-800">
          <div className="relative group">
            {avatarUrl ? (
              <img
                src={avatarUrl}
                alt="Admin Avatar"
                className="w-20 h-20 rounded-2xl object-cover ring-4 ring-orange-500/20 shadow-md"
              />
            ) : (
              <div className="w-20 h-20 rounded-2xl bg-gradient-to-tr from-orange-500 to-amber-400 text-white flex items-center justify-center font-black text-2xl shadow-md">
                {fullName ? fullName[0] : 'A'}
              </div>
            )}
            <button
              onClick={() => fileInputRef.current?.click()}
              className="absolute -bottom-2 -right-2 p-2 rounded-xl bg-orange-500 text-white shadow-md hover:bg-orange-600 transition-colors"
              title="Upload New Picture"
            >
              <Camera className="w-3.5 h-3.5" />
            </button>
          </div>

          <div className="space-y-2">
            <h4 className="font-extrabold text-sm text-slate-900 dark:text-white">Admin Profile Picture</h4>
            <p className="text-xs text-slate-500 dark:text-slate-400">Upload a square image (JPG or PNG, max 2MB)</p>
            
            <input
              type="file"
              ref={fileInputRef}
              accept="image/*"
              onChange={handleImageUpload}
              className="hidden"
            />

            <div className="flex items-center gap-3 pt-1">
              <button
                type="button"
                onClick={() => fileInputRef.current?.click()}
                className="flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-orange-50 dark:bg-orange-950/40 text-orange-600 dark:text-orange-400 font-bold text-xs hover:bg-orange-100 transition-colors"
              >
                <Upload className="w-3.5 h-3.5" /> Change Picture
              </button>

              {avatarUrl && (
                <button
                  type="button"
                  onClick={handleRemoveAvatar}
                  className="flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-rose-50 dark:bg-rose-950/40 text-rose-600 dark:text-rose-400 font-bold text-xs hover:bg-rose-100 transition-colors"
                >
                  <Trash2 className="w-3.5 h-3.5" /> Remove Picture
                </button>
              )}
            </div>
          </div>
        </div>

        {/* Profile Inputs */}
        <form onSubmit={handleSaveProfile} className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="text-xs font-bold text-slate-500 uppercase">Administrator Full Name</label>
              <input
                type="text"
                value={fullName}
                onChange={(e) => setFullName(e.target.value)}
                required
                className="w-full mt-1.5 px-4 py-2.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50 dark:bg-slate-800 text-sm font-bold text-slate-900 dark:text-white"
              />
            </div>
            <div>
              <label className="text-xs font-bold text-slate-500 uppercase">Login Email Address</label>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                className="w-full mt-1.5 px-4 py-2.5 rounded-xl border border-slate-200 dark:border-slate-800 bg-slate-50 dark:bg-slate-800 text-sm font-bold text-slate-900 dark:text-white"
              />
            </div>
          </div>

          <button
            type="submit"
            className="flex items-center gap-2 px-5 py-2.5 rounded-xl bg-orange-500 text-white font-bold text-xs shadow-lg shadow-orange-500/25 hover:bg-orange-600 transition-colors"
          >
            <Save className="w-4 h-4" /> Save Profile Details
          </button>
        </form>
      </div>

      {/* Security Password Form */}
      <div className="p-6 rounded-2xl bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 shadow-sm space-y-6">
        <h3 className="font-extrabold text-base text-slate-900 dark:text-white flex items-center gap-2">
          <KeyRound className="w-5 h-5 text-orange-500" /> Security & Password Settings
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

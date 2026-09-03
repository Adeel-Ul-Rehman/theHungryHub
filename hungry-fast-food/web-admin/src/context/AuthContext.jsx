import React, { createContext, useContext, useState, useEffect } from 'react';

const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const [token, setToken] = useState(() => localStorage.getItem('web_admin_token') || null);
  const [admin, setAdmin] = useState(() => {
    const saved = localStorage.getItem('web_admin_user');
    return saved ? JSON.parse(saved) : {
      fullName: 'Restaurant Owner',
      email: 'admin@thehungryhub.shop',
      role: 'Owner / Administrator',
      avatarUrl: null
    };
  });

  const [isAuthenticated, setIsAuthenticated] = useState(() => Boolean(localStorage.getItem('web_admin_token')));
  const [showLogoutModal, setShowLogoutModal] = useState(false);

  const login = async (email, password) => {
    // Generate simulated JWT token for admin session
    const mockJwt = `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.${btoa(JSON.stringify({ email, exp: Date.now() + 86400000 }))}.signature`;
    
    const userObj = {
      fullName: 'Restaurant Owner',
      email: email || 'admin@thehungryhub.shop',
      role: 'Owner / Administrator',
      avatarUrl: admin.avatarUrl || null
    };

    setToken(mockJwt);
    setAdmin(userObj);
    setIsAuthenticated(true);
    localStorage.setItem('web_admin_token', mockJwt);
    localStorage.setItem('web_admin_user', JSON.stringify(userObj));
    return true;
  };

  const confirmLogout = () => {
    setToken(null);
    setIsAuthenticated(false);
    setShowLogoutModal(false);
    localStorage.removeItem('web_admin_token');
    localStorage.removeItem('web_admin_user');
  };

  const updateProfile = (updatedData) => {
    const newAdmin = { ...admin, ...updatedData };
    setAdmin(newAdmin);
    localStorage.setItem('web_admin_user', JSON.stringify(newAdmin));
  };

  return (
    <AuthContext.Provider value={{ 
      token, 
      admin, 
      isAuthenticated, 
      login, 
      confirmLogout, 
      showLogoutModal, 
      setShowLogoutModal, 
      updateProfile 
    }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => useContext(AuthContext);

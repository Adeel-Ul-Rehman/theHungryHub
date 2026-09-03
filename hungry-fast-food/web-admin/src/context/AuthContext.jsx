import React, { createContext, useContext, useState, useEffect } from 'react';

const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const [admin, setAdmin] = useState(() => {
    const saved = localStorage.getItem('web_admin_user');
    return saved ? JSON.parse(saved) : {
      fullName: 'Restaurant Owner',
      email: 'admin@thehungryhub.shop',
      phone: '+92 300 1234567',
      role: 'Owner / Administrator'
    };
  });

  const [isAuthenticated, setIsAuthenticated] = useState(true); // Default true for instant demo

  const login = (email, password) => {
    const user = {
      fullName: 'Restaurant Owner',
      email: email || 'admin@thehungryhub.shop',
      phone: '+92 300 1234567',
      role: 'Owner / Administrator'
    };
    setAdmin(user);
    setIsAuthenticated(true);
    localStorage.setItem('web_admin_user', JSON.stringify(user));
    return true;
  };

  const logout = () => {
    setIsAuthenticated(false);
    localStorage.removeItem('web_admin_user');
  };

  const updateProfile = (updatedData) => {
    const newAdmin = { ...admin, ...updatedData };
    setAdmin(newAdmin);
    localStorage.setItem('web_admin_user', JSON.stringify(newAdmin));
  };

  return (
    <AuthContext.Provider value={{ admin, isAuthenticated, login, logout, updateProfile }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => useContext(AuthContext);

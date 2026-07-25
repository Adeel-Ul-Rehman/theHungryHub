// E:\hungryHub\hungry-fast-food\website\frontend\src\hooks\useLocalStorage.js

import { useState, useEffect } from 'react';

export function useLocalStorage(key, initialValue) {
    // Get stored value
    const readValue = () => {
        try {
            const item = localStorage.getItem(key);
            return item ? JSON.parse(item) : initialValue;
        } catch (error) {
            console.warn(`Error reading localStorage key "${key}":`, error);
            return initialValue;
        }
    };

    const [storedValue, setStoredValue] = useState(readValue);

    // Update stored value
    const setValue = (value) => {
        try {
            const valueToStore = value instanceof Function ? value(storedValue) : value;
            setStoredValue(valueToStore);
            localStorage.setItem(key, JSON.stringify(valueToStore));
        } catch (error) {
            console.warn(`Error setting localStorage key "${key}":`, error);
        }
    };

    // Listen to changes from other tabs
    useEffect(() => {
        const handleStorageChange = (event) => {
            if (event.key === key && event.newValue) {
                setStoredValue(JSON.parse(event.newValue));
            }
        };

        window.addEventListener('storage', handleStorageChange);
        return () => window.removeEventListener('storage', handleStorageChange);
    }, [key]);

    return [storedValue, setValue];
}
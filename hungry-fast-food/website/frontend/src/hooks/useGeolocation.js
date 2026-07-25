// E:\hungryHub\hungry-fast-food\website\frontend\src\hooks\useGeolocation.js

import { useState, useEffect } from 'react';

export function useGeolocation() {
    const [location, setLocation] = useState({
        latitude: null,
        longitude: null,
        accuracy: null,
        error: null,
        loading: true,
    });

    useEffect(() => {
        if (!navigator.geolocation) {
            setLocation(prev => ({
                ...prev,
                error: 'Geolocation is not supported by your browser',
                loading: false,
            }));
            return;
        }

        const handleSuccess = (position) => {
            setLocation({
                latitude: position.coords.latitude,
                longitude: position.coords.longitude,
                accuracy: position.coords.accuracy,
                error: null,
                loading: false,
            });
        };

        const handleError = (error) => {
            setLocation(prev => ({
                ...prev,
                error: error.message,
                loading: false,
            }));
        };

        const options = {
            enableHighAccuracy: true,
            timeout: 10000,
            maximumAge: 60000,
        };

        const watchId = navigator.geolocation.watchPosition(
            handleSuccess,
            handleError,
            options
        );

        return () => {
            navigator.geolocation.clearWatch(watchId);
        };
    }, []);

    return location;
}
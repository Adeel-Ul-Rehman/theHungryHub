// E:\hungryHub\hungry-fast-food\website\frontend\src\hooks\usePolling.js

import { useState, useEffect, useRef } from 'react';

export function usePolling(fetchFn, interval = 10000, dependencies = []) {
    const [data, setData] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const intervalRef = useRef(null);

    const fetchData = async () => {
        try {
            const result = await fetchFn();
            setData(result);
            setError(null);
        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchData();

        if (interval > 0) {
            intervalRef.current = setInterval(fetchData, interval);
        }

        return () => {
            if (intervalRef.current) {
                clearInterval(intervalRef.current);
            }
        };
    }, dependencies);

    return { data, loading, error, refetch: fetchData };
}
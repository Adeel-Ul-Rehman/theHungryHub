// E:\hungryHub\hungry-fast-food\website\frontend\src\hooks\useMenu.js

import useSWR from 'swr';
import { api } from '../services/api';

const fetcher = async (url) => {
    const res = await api.get(url);
    if (!res.success) {
        throw new Error(res.message || 'Fetch failed');
    }
    return res.data;
};

export function useMenu() {
    // SWR will auto-refetch on window focus, network reconnection, and caches data locally for offline usage
    const { data: categories, error: catsError, mutate: mutateCats } = useSWR('/menu/categories', fetcher, {
        revalidateOnFocus: true,
        revalidateOnReconnect: true,
        fallbackData: JSON.parse(localStorage.getItem('cached_categories') || '[]'),
        onSuccess: (data) => {
            localStorage.setItem('cached_categories', JSON.stringify(data));
        }
    });

    const { data: products, error: prodsError, mutate: mutateProds } = useSWR('/menu/products?is_active=true', fetcher, {
        revalidateOnFocus: true,
        revalidateOnReconnect: true,
        fallbackData: JSON.parse(localStorage.getItem('cached_products') || '[]'),
        onSuccess: (data) => {
            localStorage.setItem('cached_products', JSON.stringify(data));
        }
    });

    const { data: deals, error: dealsError, mutate: mutateDeals } = useSWR('/menu/deals?is_active=true', fetcher, {
        revalidateOnFocus: true,
        revalidateOnReconnect: true,
        fallbackData: JSON.parse(localStorage.getItem('cached_deals') || '[]'),
        onSuccess: (data) => {
            localStorage.setItem('cached_deals', JSON.stringify(data));
        }
    });

    const isLoading = !categories && !products && !deals && !catsError && !prodsError && !dealsError;

    return {
        categories: categories || [],
        products: products || [],
        deals: deals || [],
        isLoading,
        isError: catsError || prodsError || dealsError,
        mutate: () => {
            mutateCats();
            mutateProds();
            mutateDeals();
        }
    };
}

// E:\hungryHub\hungry-fast-food\website\frontend\src\hooks\useMenuData.js

import useSWR from 'swr';
import { api } from '../services/api';

const fetcher = (url) => api.swrFetcher(url);

export function useMenuData() {
    const config = {
        refreshInterval: 60000, // Auto-refresh every 60 seconds
        revalidateOnFocus: true,
        revalidateOnReconnect: true,
        keepPreviousData: true
    };

    // 1. Categories SWR Hook
    const { data: categories, error: catsError, mutate: mutateCats } = useSWR('/menu/categories', fetcher, {
        ...config,
        fallbackData: (() => {
            try {
                return JSON.parse(localStorage.getItem('cached_categories') || '[]');
            } catch {
                return [];
            }
        })(),
        onSuccess: (data) => {
            localStorage.setItem('cached_categories', JSON.stringify(data));
        }
    });

    // 2. Products SWR Hook
    const { data: products, error: prodsError, mutate: mutateProds } = useSWR('/menu/products?is_active=true', fetcher, {
        ...config,
        fallbackData: (() => {
            try {
                return JSON.parse(localStorage.getItem('cached_products') || '[]');
            } catch {
                return [];
            }
        })(),
        onSuccess: (data) => {
            localStorage.setItem('cached_products', JSON.stringify(data));
        }
    });

    // 3. Deals SWR Hook
    const { data: deals, error: dealsError, mutate: mutateDeals } = useSWR('/menu/deals?is_active=true', fetcher, {
        ...config,
        fallbackData: (() => {
            try {
                return JSON.parse(localStorage.getItem('cached_deals') || '[]');
            } catch {
                return [];
            }
        })(),
        onSuccess: (data) => {
            localStorage.setItem('cached_deals', JSON.stringify(data));
        }
    });

    // 4. Featured Deal SWR Hook
    const { data: featuredDeal, error: featuredError, mutate: mutateFeatured } = useSWR('/menu/deals/featured', fetcher, {
        ...config,
        fallbackData: (() => {
            try {
                return JSON.parse(localStorage.getItem('cached_featured_deal') || 'null');
            } catch {
                return null;
            }
        })(),
        onSuccess: (data) => {
            localStorage.setItem('cached_featured_deal', JSON.stringify(data));
        }
    });

    const isLoading = !categories && !products && !deals && !featuredDeal && !catsError && !prodsError && !dealsError && !featuredError;
    const isError = catsError || prodsError || dealsError || featuredError;

    return {
        categories: categories || [],
        products: products || [],
        deals: deals || [],
        featuredDeal: featuredDeal || null,
        isLoading,
        isError,
        mutate: () => {
            mutateCats();
            mutateProds();
            mutateDeals();
            mutateFeatured();
        }
    };
}

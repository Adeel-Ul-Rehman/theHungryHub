-- Migration: Add maps_url column to orders table
-- Run this script if you have an existing database

-- Add maps_url column for Google Maps location links
ALTER TABLE orders 
ADD COLUMN IF NOT EXISTS maps_url VARCHAR(500);

-- Add column comment
COMMENT ON COLUMN orders.maps_url IS 'Google Maps link for delivery orders';

-- Create index for faster queries on delivery orders
CREATE INDEX IF NOT EXISTS idx_orders_maps_url 
ON orders(maps_url) 
WHERE maps_url IS NOT NULL;
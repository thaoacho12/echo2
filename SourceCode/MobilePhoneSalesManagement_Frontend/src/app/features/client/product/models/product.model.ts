export interface Product {
    productId: number;
    name: string;
    description: string;
    price: number;
    oldPrice: number;
    stockQuantity: number; // default values need to be handled elsewhere
    brandId?: number; // nullable types use "?"
    imageUrl: string;
    manufacturer: string;
    isActive: boolean; // default values need to be handled elsewhere
    color: string;
    discount: number; // default values need to be handled elsewhere
    createdAt: Date; // default values need to be handled elsewhere
    updatedAt: Date; // default values need to be handled elsewhere
}
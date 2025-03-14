import { Brand } from "../../brand-management/models/brand.model";
import { productSpecifications } from "../../specification-type-management/models/specificationType";
import { Image } from "../../../admin/image/models/image";

export interface Product {
    productId: string;
    name: string;
    description: string;
    price: number;
    oldPrice: number;
    stockQuantity: number;
    brandId: number;
    image: Image;
    imageId: number;
    manufacturer: string;
    isActive: boolean;
    colors: string;
    discount: number;
    createdAt: Date;
    updatedAt: Date;
    brand: Brand;
    productSpecifications: productSpecifications[];
}

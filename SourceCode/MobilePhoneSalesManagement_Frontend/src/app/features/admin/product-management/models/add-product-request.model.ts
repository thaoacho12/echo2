import { RequestProductSpecifications } from "../../specification-type-management/models/add-specificationType-request";
import { RequestImage } from "../../image/models/add-image-request.model";

export interface RequestProduct {
    name: string;
    description: string;
    price: number;
    oldPrice: number;
    stockQuantity: number;
    brandId: number;
    imageId: number;
    image: RequestImage;
    manufacturer: string;
    isActive: boolean;
    colors: string;
    discount: number;
    productSpecifications: RequestProductSpecifications[];
}

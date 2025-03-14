
import { Image } from "../../../admin/image/models/image";

export interface Brand {
    brandId: string;
    name: string;
    image: Image;
    isActive: boolean;
    productCount: number;
    imageId: number;
    createdAt: Date;
    updatedAt: Date;
}

export interface PagedResult<T> {
    currentPage: number;
    totalPages: number;
    pageSize: number;
    totalCount: number;
    items: T[];
}

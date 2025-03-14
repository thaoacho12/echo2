import { RequestImage } from "../../image/models/add-image-request.model";
export interface RequestBrand {
    name: string;
    isActive: boolean;
    imageId: number;
    image: RequestImage;
}
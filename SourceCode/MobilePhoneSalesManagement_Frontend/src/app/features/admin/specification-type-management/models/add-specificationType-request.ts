import { specificationType } from "./specificationType";

export interface RequestSpecificationType {
    name: string;
}

export interface RequestProductSpecifications {
    specificationTypeId: string;
    value: string;
    specificationType: specificationType;
}


export interface ProductSpecificationWithEditMode extends RequestProductSpecifications {
    editMode: boolean;
}
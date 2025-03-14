export interface specificationType {
    specificationTypeId: string;
    name: string;
    createdAt?: Date;
    updatedAt?: Date;
}

export interface productSpecifications {
    productId: string;
    specificationTypeId: string;
    value: string;
    createdAt: Date;
    updatedAt: Date;
    specificationType: specificationType;
}

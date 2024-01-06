import { Group } from "./group";

export interface Adventure {
    name: string;
    picture: string;
    groups: Group[];
    steps: AdventureStep[];
}

export interface AdventureStep {
    name: string;
}
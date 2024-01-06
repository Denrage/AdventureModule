import { User } from "./user";

export interface Group {
    name: string;
    users: User[];
    color: string;
    step: string;
}
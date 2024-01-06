export interface User {
    username: string;
    discord: string;
    discordId: string;
    profilePicture: string;
    ingameUser: IngameUser;
}

export interface IngameUser {
    accountName: string;
    characterName: string;
    positionX: number;
    positionY: number;
}
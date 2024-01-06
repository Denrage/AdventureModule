import { Injectable } from '@angular/core';
import { User } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  constructor() { }

  public getUsers(): User[] {
    return [
      {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: "", ingameUser: { accountName: "honky.3864", characterName: "Athena Tholaen", positionX: 48000, positionY: 30720 }},
      {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: "", ingameUser: { accountName: "Denrage.3864", characterName: "Seria", positionX: 50000, positionY: 30720 }},
      {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: "", ingameUser: { accountName: "whitetiger.7425", characterName: "Gwendolyn", positionX: 48000, positionY: 20720 }},
      {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: "", ingameUser: { accountName: "xTaddelx.SomeNumber", characterName: "KeineAhnung", positionX: 38000, positionY: 30720 }},
    ]
  }
}

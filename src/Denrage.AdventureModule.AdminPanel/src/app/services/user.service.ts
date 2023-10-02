import { Injectable } from '@angular/core';
import { User } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  constructor() { }

  public getUsers(): User[] {
    return [
      {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: ""},
      {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: ""},
      {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: ""},
      {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: ""},
      {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: ""},
      {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: ""},
      {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: ""},
      {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: ""},
      {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: ""},
      {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: ""},
      {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: ""},
      {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: ""},
      {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: ""},
      {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: ""},
      {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: ""},
      {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: ""},
      {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: ""},
      {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: ""},
      {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: ""},
      {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: ""},
      {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: ""},
      {username: "hello", discord: "helloworld", profilePicture: "https://picsum.photos/200", discordId: ""},
    ]
  }
}

import { Component, OnInit } from '@angular/core';
import { User } from 'src/app/models/user';
import { UserService } from 'src/app/services/user.service';

@Component({
  selector: 'app-participants',
  templateUrl: './participants.component.html',
  styleUrls: ['./participants.component.scss']
})
export class ParticipantsComponent implements OnInit {
  items: User[] = [];
  constructor(private userService: UserService) { }
  
  ngOnInit(): void {
    this.items = this.userService.getUsers();
  }

  
}

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ParticipantsComponent } from './admin/participants/participants.component';
import { AdventuresComponent } from './admin/adventures/adventures.component';
import { AdventureDetailComponent } from './admin/adventures/adventure-detail/adventure-detail.component';

const routes: Routes = [
  { path: 'admin/participants', component: ParticipantsComponent },
  { path: 'admin/adventures', component: AdventuresComponent },
  { path: 'admin/adventures/detail', component: AdventureDetailComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }

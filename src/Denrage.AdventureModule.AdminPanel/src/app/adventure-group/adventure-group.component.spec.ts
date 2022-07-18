import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdventureGroupComponent } from './adventure-group.component';

describe('AdventureGroupComponent', () => {
  let component: AdventureGroupComponent;
  let fixture: ComponentFixture<AdventureGroupComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ AdventureGroupComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdventureGroupComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

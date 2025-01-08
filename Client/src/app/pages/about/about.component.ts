import { Component, ElementRef, QueryList, ViewChildren } from '@angular/core';

@Component({
    selector: 'app-about',
    imports: [],
    templateUrl: './about.component.html',
    styleUrl: './about.component.scss'
})
export class AboutComponent {
  @ViewChildren ('animatedText') animatedTexts!: QueryList<ElementRef>;
  observer:IntersectionObserver = new IntersectionObserver((entries) => {
    entries.forEach((entry) => {
      if (entry.isIntersecting) {
        entry.target.classList.add('show');
      } else {
        entry.target.classList.remove('show');
      }
    });
  });
  ngAfterViewInit() {
    this.animatedTexts.forEach((el) => {
      this.observer.observe(el.nativeElement);
    });
  }
}

#include <stdio.h>
#include <stdlib.h>

struct elem;

struct stack {
  struct stack_elem *head; // начальный элемент
  int kol; // количество элементов
};

struct stack_elem // Элемент
{
    int num; // порядковый номер
    int value; // сохраняемое значение
    struct stack_elem *next; // указатель на след. элемент
}; 

void init(struct stack *stk) {
  stk->kol = 0;
}

void push(struct stack *stk, int val) {

  struct stack_elem *elem;
  
  elem = (struct stack_elem*)malloc(sizeof(struct stack_elem));

  __atomic_store (&elem->value, &val,  __ATOMIC_RELAXED);
  __atomic_store (&elem->num, &stk->kol,  __ATOMIC_RELAXED);

  if (stk->kol == 0) {
    elem->next = NULL;
    __atomic_store (&stk->head, &elem,  __ATOMIC_RELAXED); 
  }
  else
  {
    __atomic_store (&elem->next, &stk->head,  __ATOMIC_RELAXED); // текущий стек вставляем за новым элементом
    __atomic_store (&stk->head, &elem,  __ATOMIC_RELAXED); // новый элемент, вставляем в начало списка
  }
 
  stk->kol++;
   
  printf("added elem to stack: %d; ", val);
  printf("stack length after insert: %d\n", stk->kol);
}
 
int pop(struct stack *stk) {
  struct stack_elem *elem;
  if (stk->kol > 0) {
    __atomic_store (&elem, &stk->head,  __ATOMIC_RELAXED); // взять верхний элемент
    __atomic_store (&stk->head , &elem->next,  __ATOMIC_RELAXED); // убрать верхний элемент
 
    stk->kol--;
    int value;
    __atomic_store (&value, &elem->value,  __ATOMIC_RELAXED); // сохранить значение
 
    free(elem);
    return value;
  }
  else
  {
    return -1;
  }
}

int main() {
  struct stack *stk;
  stk = (struct stack*)malloc(sizeof(struct stack));
  init(stk);
  push(stk, 1);
  push(stk, 7);
  push(stk, 3);
  push(stk, 9);
  push(stk, 0);
  push(stk, 4);

  printf("pop elem = %i\n", pop(stk));
  printf("pop elem = %i\n", pop(stk));
  printf("pop elem = %i\n", pop(stk));
  printf("pop elem = %i\n", pop(stk));
  printf("pop elem = %i\n", pop(stk));
  printf("pop elem = %i\n", pop(stk));

  return 0;
}

  

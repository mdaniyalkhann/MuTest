#include <iostream>
#include "assert.h"
using namespace std;

void functionWithCout()
{
	cout << (1 == 1);
}

void functionWithCinFunction()
{
	cin.clear(0, 1 == 1);
}

void functionWithCoutFunction()
{
	cout.clear(0, 1 == 1);
}

void functionWithPrintf()
{
	printf((char*)(1 == 1));
}

void functionWithfPrintf()
{
	FILE *fp;
	fprintf(fp, (char*)(1 == 1));
}

void functionWithsPrintf()
{
	char *c;
	sprintf(c, (char*)(1 == 1));
}

void functionWithAssert() {
	assert(1 == 1);
}

void functionWithMalloc() {
	malloc(1 == 1 ? 10 : 5);
}

void functionWithCalloc() {
	calloc(1 == 1 ? 10 : 5, 30);
}

void functionWithRealloc() {
	realloc(1 == 1 ? 10 : 5, 30);
}

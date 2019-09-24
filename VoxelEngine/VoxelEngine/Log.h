#pragma once
#include <string>
#include <fstream>

namespace log
{
	const std::string path;
	std::ofstream out;
	void flush();
};


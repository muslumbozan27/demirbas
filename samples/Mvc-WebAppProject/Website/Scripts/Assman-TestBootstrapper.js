///<reference path="~/scripts/jquery-1.4.1-vsdoc.js" />

(function ()
{
	$(window).load(function ()
	{
		$("dd.test").not(".pass").html("FAILED").css("color", "red");
		$("dd.test.pass").html("PASSED").css("color", "green");
	});
})();
<?php
	function get_headers_from_curl_response($response)
	{
		$headers = array();
		$header_text = substr($response, 0, strpos($response, "\r\n\r\n"));
		foreach (explode("\r\n", $header_text) as $i => $line)
			if ($i === 0s
				$headers['http_code'] = $line;
			else
			{
				list ($key, $value) = explode(': ', $line);
				$headers[$key] = $value;
			}
		return $headers;
	}
	$cookie_jar = tempnam('/tmp','cookie');
	$ResourceID = $_GET['ResourceID'];
  
	$c = curl_init('http://oxidemod.org/');
	curl_setopt($c, CURLOPT_RETURNTRANSFER, 1);
	curl_setopt($c, CURLOPT_COOKIEFILE, $cookie_jar);
	curl_setopt($c, CURLOPT_COOKIEJAR, $cookie_jar);
	$page = curl_exec($c);
	curl_close($c);
	$c = curl_init('http://oxidemod.org/login/login/');
	curl_setopt($c, CURLOPT_POST, 1);
	curl_setopt($c, CURLOPT_POSTFIELDS, 'login=FORUMUSERNAME&password=FORUMPASSWORD');
	curl_setopt($c, CURLOPT_RETURNTRANSFER, 1);
	curl_setopt($c, CURLOPT_FOLLOWLOCATION, true);
	curl_setopt($c, CURLOPT_COOKIEFILE, $cookie_jar);
	curl_setopt($c, CURLOPT_COOKIEJAR, $cookie_jar);
	$page = curl_exec($c);
	curl_close($c);
  $c = curl_init('http://oxidemod.org/plugins/' . $ResourceID  . '/');
	curl_setopt($c, CURLOPT_RETURNTRANSFER, 1);
	curl_setopt($c, CURLOPT_COOKIEFILE, $cookie_jar);
	curl_setopt($c, CURLOPT_COOKIEJAR, $cookie_jar);
	curl_setopt($c, CURLOPT_FOLLOWLOCATION, true);
	$page = curl_exec($c);
	curl_close($c);
	$dom = new DOMDocument();
	@$dom->loadHTML($page);
	$xpath = new DOMXPath($dom);
	$node = $xpath->query("//*[@id=\"versionInfo\"]/ul/li/label//a")->item(0);
	$DownloadUrl = "http://oxidemod.org/" . $node->getAttribute("href");
	$c = curl_init($DownloadUrl);
	curl_setopt($c, CURLOPT_RETURNTRANSFER, 1);
	curl_setopt($c, CURLOPT_COOKIEFILE, $cookie_jar);
	curl_setopt($c, CURLOPT_COOKIEJAR, $cookie_jar);
	curl_setopt($c, CURLOPT_FOLLOWLOCATION, true);
	curl_setopt($c, CURLOPT_VERBOSE, 1);
	curl_setopt($c, CURLOPT_HEADER, 1);
	$response  = curl_exec($c);
	$header_size = curl_getinfo($c, CURLINFO_HEADER_SIZE);
	$header = get_headers_from_curl_response($response);
	$body = substr($response, $header_size);
	curl_close($c);
	header('Content-Type: ' . $header["Content-Type"]);
	header('Content-Length: ' . $header["Content-Length"]);
	header('Content-Disposition: ' . $header["Content-Disposition"]);
	echo $body;
  
	unlink($cookie_jar);
?>

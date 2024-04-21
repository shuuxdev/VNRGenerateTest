import React from 'react';
import { FiSearch } from 'react-icons/fi';
import { useState } from 'react';
const SearchBar = () => {
    const [query, setQuery] = useState('');
    const [results, setResults] = useState([]);

    const fetchListOfPagesAsync = async () => {
        let projectID = "41abf2e0-a388-43de-9f5e-666da177bd5d";
        let repositoryID = "15002d83-9470-4b0e-9a8d-7a655d9af004";
        let path = ""
        let branch = "develop-main";
        // Construct the URL for the TFS REST API
        let apiUrl = `http://172.21.35.3:8080/tfs/HRMCollection/${projectID}/_apis/git/repositories/${repositoryID}/Items?path=${path}&recursionLevel=0&includeContentMetadata=true&latestProcessedChange=false&download=false&versionDescriptor%5BversionType%5D=branch&versionDescriptor%5Bversion%5D=${branch}&includeContent=true`;
        let data = await fetch("http://172.21.35.3:8080/tfs/HRMCollection/41abf2e0-a388-43de-9f5e-666da177bd5d/_apis/git/repositories/15002d83-9470-4b0e-9a8d-7a655d9af004/itemsBatch", {
            "credentials": "include",
            "headers": {
                "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:125.0) Gecko/20100101 Firefox/125.0",
                "Accept": "application/json;api-version=4.1;excludeUrls=true",
                "Accept-Language": "en,en-US;q=0.5",
                "Content-Type": "application/json",
                "X-VSS-ReauthenticationAction": "Suppress",
                "X-TFS-Session": "60537a4f-1a8e-43af-baa3-98b076f59ee7",
                "X-Requested-With": "XMLHttpRequest"
            },
            "referrer": "http://172.21.35.3:8080/tfs/HRMCollection/_git/HRM9/?path=%2FMain%2FSource%2FPresentation%2FHRM.Presentation.Main%2FViews%2FAtt_ApprovedLeaveday&version=GBdevelop-main&_a=contents",
            "body": "{\"itemDescriptors\":[{\"path\":\"/Main/Source/Presentation/HRM.Presentation.Main/Views/Att_ApprovedLeaveday\",\"version\":\"develop-main\",\"versionType\":\"branch\",\"recursionLevel\":4}],\"includeContentMetadata\":true}",
            "method": "POST",
        }).then(res => res.json());
    }

    const handleInputChange = async (event) => {
        const inputValue = event.target.value;
        setQuery(inputValue);

        if (results.length == 0)
        {
            let data = await fetchListOfPagesAsync();
            console.log(data);
            setResults(data);
        }
            
        // Perform search based on the input value

    };

    return (
        <div className="relative w-[400px]">
            <input
                type="text"
                placeholder={"Nhập tên màn hình"}
                className="w-full bg-white border-[1px] border-solid border-gray-200 py-2 pl-10 pr-4 rounded-full shadow-sm  focus:outline-none focus:shadow-outline"
                value={query}
                onChange={handleInputChange}
            />
            <div className="absolute top-0 left-0 flex items-center h-full px-3">
                <FiSearch className="text-gray-400" />
            </div>

            {/* Dropdown of search results */}
            {query && results.length > 0 && (
                <ul className="absolute z-10 mt-2 w-full bg-white border rounded-md shadow-md">
                    {results.map((result, index) => (
                        <li
                            key={index}
                            className="py-2 px-4 cursor-pointer hover:bg-gray-100"
                            onClick={() => {
                                // Handle selection of a result
                                console.log('Selected:', result);
                                // You can perform any action here, such as updating the input field with the selected result
                            }}
                        >
                            {result}
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
};

export default SearchBar;
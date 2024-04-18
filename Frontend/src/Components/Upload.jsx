import { useEffect, useState } from "react"

const Upload = () => {
    const [selectedFile, setSelectedFile] = useState(null);
    const [previewImage, setPreviewImage] = useState(null);

    const handleFileChange = (e) => {
        const file = e.target.files[0];
        setSelectedFile(file);

        // Preview the selected image

    };

    return (
        <div className="max-w-lg mx-auto mt-10 bg-white p-6 rounded-lg shadow-md">
            <div className="flex items-center justify-center mb-6">
                <Dragzone/>
            </div>
           
        </div>
    );
};

const Dragzone = () => {
    const [dragging, setDragging] = useState(false);
    const [file, setFile] = useState(null);

    const handleDragEnter = (e) => {
        e.preventDefault();
        setDragging(true);
    };

    const handleDragLeave = (e) => {
        e.preventDefault();
        setDragging(false);
    };

    const handleDrop = (e) => {
        e.preventDefault();
        setDragging(false);
        const droppedFiles = e.dataTransfer.files;
        if (droppedFiles.length > 0) {
            const droppedFile = droppedFiles[0];
            setFile(droppedFile);
        } else {
            // Handle file drop from VSCode or Visual Studio
            const filePath = e.dataTransfer.getData("text/plain");
            console.log(filePath)
            const fileName = filePath.substring(filePath.lastIndexOf("/") + 1);
            fetch(filePath)
                // .then((response) => response.blob())
                // .then((blob) => {
                //     const newFile = new File([blob], fileName);
                //     setFile(newFile);
                //     console.log(newFile)
                // })
                // .catch((error) => {
                //     console.log(error);
                //     // Handle the error and display an appropriate message to the user
                // });
        }
    };

    const handleFileChange = (e) => {
        console.log(e.target.files)
        
        const newFile = e.target.files[0];
        setFile(newFile);
        //previewFile(newFile);
    };

    const previewFile = (file) => {
        const reader = new FileReader();
        reader.onloadend = () => {
            // Do something with the preview image URL
        };
        reader.readAsDataURL(file);
    };
    useEffect(() => {

    }, [file])

    return (
        <div
            className={`h-64 w-64 border-2 border-dashed rounded-lg flex items-center justify-center text-gray-500 ${dragging ? 'border-blue-500' : ''
                }`}
            onDragOver={(e) => e.preventDefault()}
            onDragEnter={handleDragEnter}
            onDragLeave={handleDragLeave}
            onDrop={handleDrop}
        >
            {!file && (
                <div>
                    <p>Thả file vào đây</p>
                    <p>hoặc</p>
                    <label htmlFor="file-upload" className="cursor-pointer">
                        <input
                            id="file-upload"
                            type="file"
                            className="sr-only"
                            onChange={handleFileChange}
                        />
                        <span className="text-blue-500 font-semibold">Chọn</span> trực tiếp từ thư mục
                    </label>
                </div>
            )}
            {file && (
                <div>
                    <p>{file.name}</p>
                </div>
            )}
        </div>
    );
};


export {Upload};
import { transform, useAnimate, useMotionValue } from "framer-motion"
import { useEffect ,useState} from "react";
import { FaAnglesLeft,FaAnglesRight } from "react-icons/fa6";
import { motion ,useAnimationControls } from "framer-motion";
import { Editor } from "@monaco-editor/react";
import SearchBar from "./Searchbar";
export const Detail = () => {
    const [isDetailOpen, setIsDetailOpen] = useState(false);
    const controls = useAnimationControls()
    const arrowControl = useAnimationControls();
    useEffect(() => {
        let animate = async  () => {
            let transX = isDetailOpen ? '0px': '100%';
            const rotateDirection = isDetailOpen ? 180 : 0;
            
            controls.start({x: transX}, {duration: 0.3})
            await arrowControl.start({rotate: rotateDirection}, {duration: 0.3})
        }
        animate();
    }, [isDetailOpen])
    const ToggleDetail = () => {
        setIsDetailOpen(!isDetailOpen);
    }
    const editorOptions = {
        lineNumbers: value => `${value + 100}` // Adjust the start line number here
    };
    return (
            
        <motion.div animate={controls} className="relative shadow-md w-full h-screen bg-red-50">

            <div onClick={ToggleDetail} className="cursor-pointer hover:scale-110 hover:shadow-lg absolute bg-white top-[50%] translate-y-[-50%] left-0 translate-x-[-120%] rounded-full shadow-md flex justify-center items-center p-[10px]">
                <motion.div animate={arrowControl}>
                    <FaAnglesLeft className="text-blue-400"/>
                </motion.div>
            </div>
            
                <Editor theme="dark" height="100%" options={editorOptions} defaultLanguage="csharp" defaultValue="// some comment" />
        </motion.div>
    )
}
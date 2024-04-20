import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './App.css'
import { Upload } from './Components/Upload'
import { Detail } from './Components/Detail'
import SearchBar from './Components/Searchbar'
import { Button } from 'antd'
function App() {
  const [count, setCount] = useState(0)

  return (
    <div className='flex  w-[100vw] overflow-hidden'>
      
      <div className='flex-1'>
        <div className='toolbar flex items-center gap-[10px] p-[10px] h-[60px]'>
          <Button type='default'>Chạy tất cả</Button>
          <Button type='default'>Cấu hình</Button>
          <Button type='default'>Chọn</Button>
          <SearchBar/>
        </div>
      {/* <Upload/> */}
      </div>
      <div className='min-w-[600px] '>
        <Detail/>
      </div>
    </div>
  )
}

export default App

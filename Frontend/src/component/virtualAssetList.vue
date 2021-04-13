<template>
	<div>
		<ul>
			<li v-for="asset in assets" :key="asset.id" class="mb-2">
				<virtual-asset :asset="asset" />
			</li>
		</ul>
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { Prop } from 'vue-property-decorator';
import VirtualAsset from '@/component/virtualAsset.vue';
import { VirtualAssetDto } from '@/api/market';

@Options({
	name: 'VirtualAssetList',
	components: {
		VirtualAsset,
	},
})
export default class VirtualAssetList extends Vue {
	@Prop()
	assets!: VirtualAssetDto[];
}
</script>

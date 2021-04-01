<template>
	<div>
		<ul>
			<li v-for="asset in assets" :key="asset.id" class="mb-2">
				<asset :asset="asset" />
			</li>
		</ul>
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import Asset from '@/component/asset.vue';
import { AssetDto } from '@/api/state';
import { Prop } from 'vue-property-decorator';

@Options({
	name: 'AssetList',
	components: {
		Asset,
	},
})
export default class AssetList extends Vue {
	@Prop()
	assets!: AssetDto[];
}
</script>
